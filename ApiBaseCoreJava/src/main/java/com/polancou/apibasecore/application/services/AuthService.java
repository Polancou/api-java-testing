package com.polancou.apibasecore.application.services;

import com.polancou.apibasecore.application.dtos.*;
import com.polancou.apibasecore.application.interfaces.*;
import com.polancou.apibasecore.application.utilities.ValidationUtilities;
import com.polancou.apibasecore.domain.enums.RolUsuario;
import com.polancou.apibasecore.domain.models.AuthResult;
import com.polancou.apibasecore.domain.models.UserLogin;
import com.polancou.apibasecore.domain.models.Usuario;
import com.polancou.apibasecore.infrastructure.repositories.UserRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.net.URLDecoder;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;
import java.time.ZoneOffset;

@Service
public class AuthService implements IAuthService {

    private final UserRepository userRepository;
    private final ITokenService tokenService;
    private final IExternalAuthValidator externalAuthValidator;
    private final IEmailService emailService;
    private final IEncryptionService encryptionService;

    // TODO: Inject config for frontend URL properly
    private final String frontendBaseUrl = "http://localhost:3000"; 

    public AuthService(UserRepository userRepository, 
                       ITokenService tokenService, 
                       IExternalAuthValidator externalAuthValidator, 
                       IEmailService emailService, 
                       IEncryptionService encryptionService) {
        this.userRepository = userRepository;
        this.tokenService = tokenService;
        this.externalAuthValidator = externalAuthValidator;
        this.emailService = emailService;
        this.encryptionService = encryptionService;
    }

    @Override
    @Transactional
    public AuthResult register(RegistroUsuarioDto registroDto) {
        if (userRepository.existsByEmail(registroDto.getEmail())) {
             return AuthResult.ok(null, "Si el correo es válido, recibirás un enlace de confirmación.");
        }

        String passwordEncrypted = encryptionService.encrypt(registroDto.getPassword());
        String verificationToken = tokenService.generarRefreshToken(); // Reuse random string gen

        Usuario nuevoUsuario = new Usuario(
                registroDto.getName(),
                registroDto.getEmail(),
                registroDto.getPhone(),
                RolUsuario.User,
                registroDto.getTaxId()
        );
        nuevoUsuario.establecerPasswordHash(passwordEncrypted);
        nuevoUsuario.setEmailVerificationToken(verificationToken);

        userRepository.save(nuevoUsuario);

        sendVerificationEmail(nuevoUsuario);

        return AuthResult.ok(null, "Si el correo es válido, recibirás un enlace de confirmación.");
    }

    @Override
    @Transactional
    public TokenResponseDto login(LoginUsuarioDto loginDto) {
        Usuario usuario = userRepository.findByEmail(loginDto.getEmail())
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.ValidationException("Credenciales inválidas.")); // ValidationException

        if (usuario.getPasswordHash() == null) {
             throw new com.polancou.apibasecore.application.exceptions.ValidationException("Credenciales inválidas.");
        }

        String decryptedPassword = encryptionService.decrypt(usuario.getPasswordHash());
        if (!decryptedPassword.equals(loginDto.getPassword())) {
             throw new com.polancou.apibasecore.application.exceptions.ValidationException("Credenciales inválidas.");
        }

        String accessToken = tokenService.crearToken(usuario);
        String refreshToken = tokenService.generarRefreshToken();

        usuario.setRefreshToken(refreshToken, LocalDateTime.now(ZoneOffset.UTC).plusDays(30));
        userRepository.save(usuario);

        return new TokenResponseDto(accessToken, refreshToken);
    }

    @Override
    @Transactional
    public TokenResponseDto externalLogin(ExternalLoginDto externalLoginDto) {
        if (!"google".equalsIgnoreCase(externalLoginDto.getProvider())) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("Proveedor no soportado.");
        }

        ExternalAuthUserInfo userInfo = externalAuthValidator.validateToken(externalLoginDto.getIdToken());
        if (userInfo == null) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("Token externo inválido.");
        }
        
        // ... (rest of method)

        Usuario usuario = userRepository.findByEmail(userInfo.getEmail()).orElse(null);
        if (usuario == null) {
            usuario = new Usuario(
                    userInfo.getName(),
                    userInfo.getEmail(),
                    "",
                    RolUsuario.User,
                    null
            );
            usuario.setAvatarUrl(userInfo.getPictureUrl());
            usuario.markEmailAsVerified();
            userRepository.save(usuario);
        }
        
        String accessToken = tokenService.crearToken(usuario);
        String refreshToken = tokenService.generarRefreshToken();
        
        usuario.setRefreshToken(refreshToken, LocalDateTime.now(ZoneOffset.UTC).plusDays(30));
        userRepository.save(usuario);
        
        return new TokenResponseDto(accessToken, refreshToken);
    }

    @Override
    @Transactional
    public TokenResponseDto refreshToken(String refreshToken) {
        Usuario usuario = userRepository.findAll().stream()
                .filter(u -> refreshToken.equals(u.getRefreshToken()))
                .findFirst()
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.ValidationException("Refresh token inválido."));
                
        if (usuario.getRefreshTokenExpiryTime().isBefore(LocalDateTime.now(ZoneOffset.UTC))) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("Refresh token expirado.");
        }

        String newAccessToken = tokenService.crearToken(usuario);
        String newRefreshToken = tokenService.generarRefreshToken();

        usuario.setRefreshToken(newRefreshToken, LocalDateTime.now(ZoneOffset.UTC).plusDays(30));
        userRepository.save(usuario);

        return new TokenResponseDto(newAccessToken, newRefreshToken);
    }

    @Override
    @Transactional
    public AuthResult verifyEmail(String token) {
        String decodedToken = URLDecoder.decode(token, StandardCharsets.UTF_8);
        Usuario usuario = userRepository.findByEmailVerificationToken(decodedToken) // or token?
                .orElse(userRepository.findByEmailVerificationToken(token).orElse(null));
        
        if (usuario == null) {
            return AuthResult.fail("Token de verificación inválido.");
        }

        usuario.markEmailAsVerified();
        userRepository.save(usuario);

        return AuthResult.ok(null, "Email verificado exitosamente.");
    }

    @Override
    @Transactional
    public AuthResult forgotPassword(String email) {
        userRepository.findByEmail(email).ifPresent(usuario -> {
            String resetToken = tokenService.generarRefreshToken();
            usuario.setPasswordResetToken(resetToken, LocalDateTime.now(ZoneOffset.UTC).plusHours(1));
            userRepository.save(usuario);
            sendPasswordResetEmail(usuario);
        });
        return AuthResult.ok(null, "Si existe una cuenta con ese correo, se ha enviado un enlace para restablecer la contraseña.");
    }

    @Override
    @Transactional
    public AuthResult resetPassword(ResetPasswordDto dto) {
        String decodedToken = URLDecoder.decode(dto.getToken(), StandardCharsets.UTF_8);
        // Try finding by decoded or raw
        Usuario usuario = userRepository.findByPasswordResetToken(decodedToken)
                .orElse(userRepository.findByPasswordResetToken(dto.getToken()).orElse(null));

        if (usuario == null) {
            return AuthResult.fail("El token de restablecimiento no es válido.");
        }

        if (usuario.getPasswordResetTokenExpiryTime().isBefore(LocalDateTime.now(ZoneOffset.UTC))) {
            return AuthResult.fail("El token de restablecimiento ha expirado.");
        }

        String encrypted = encryptionService.encrypt(dto.getNewPassword());
        usuario.establecerPasswordHash(encrypted);
        usuario.clearPasswordResetToken();
        userRepository.save(usuario);

        return AuthResult.ok(null, "Contraseña restablecida exitosamente.");
    }

    // Private helpers
    private void sendVerificationEmail(Usuario usuario) {
        String encodedToken = URLEncoder.encode(usuario.getEmailVerificationToken(), StandardCharsets.UTF_8);
        String link = frontendBaseUrl + "/verify-email?token=" + encodedToken;
        try {
            emailService.sendVerificationEmail(usuario.getEmail(), usuario.getName(), link);
        } catch (Exception e) {
            System.out.println("Error sending email: " + e.getMessage());
        }
    }

    private void sendPasswordResetEmail(Usuario usuario) {
        String encodedToken = URLEncoder.encode(usuario.getPasswordResetToken(), StandardCharsets.UTF_8);
        String link = frontendBaseUrl + "/reset-password?token=" + encodedToken;
        try {
            emailService.sendPasswordResetEmail(usuario.getEmail(), usuario.getName(), link);
        } catch (Exception e) {
            System.out.println("Error sending email: " + e.getMessage());
        }
    }
}
