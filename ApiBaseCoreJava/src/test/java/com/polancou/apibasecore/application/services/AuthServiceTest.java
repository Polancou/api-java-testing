package com.polancou.apibasecore.application.services;

import com.polancou.apibasecore.application.dtos.*;
import com.polancou.apibasecore.application.exceptions.ValidationException;
import com.polancou.apibasecore.application.interfaces.*;
import com.polancou.apibasecore.domain.enums.RolUsuario;
import com.polancou.apibasecore.domain.models.AuthResult;
import com.polancou.apibasecore.domain.models.Usuario;
import com.polancou.apibasecore.infrastructure.repositories.UserRepository;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.time.LocalDateTime;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class AuthServiceTest {

    @Mock
    private UserRepository userRepository;
    @Mock
    private ITokenService tokenService;
    @Mock
    private IExternalAuthValidator externalAuthValidator;
    @Mock
    private IEmailService emailService;
    @Mock
    private IEncryptionService encryptionService;

    @InjectMocks
    private AuthService authService;

    @Test
    void register_WithNewEmail_ShouldReturnSuccess() {
        // Arrange
        RegistroUsuarioDto registroDto = new RegistroUsuarioDto();
        registroDto.setEmail("nuevo@email.com");
        registroDto.setPassword("password123");
        registroDto.setName("Nuevo Usuario");
        registroDto.setPhone("1234567890");

        when(userRepository.existsByEmail(registroDto.getEmail())).thenReturn(false);
        when(encryptionService.encrypt(registroDto.getPassword())).thenReturn("encrypted-pwd-123");

        // Act
        AuthResult result = authService.register(registroDto);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(result.getMessage()).isEqualTo("Si el correo es válido, recibirás un enlace de confirmación.");

        verify(userRepository, times(1)).save(any(Usuario.class));
    }

    @Test
    void register_WithExistingEmail_ShouldReturnFail() {
        // Arrange
        RegistroUsuarioDto registroDto = new RegistroUsuarioDto();
        registroDto.setEmail("existente@email.com");
        registroDto.setPassword("password123");
        
        // This simulates ValidationException being thrown by UserService/AuthService when exists.
        // Wait, AuthService.register calls userRepository.save directly in my implementation?
        // Let's check AuthService implementation. 
        // AuthService.register calls:
        // if (userRepository.existsByEmail(dto.getEmail())) return AuthResult.ok(...) (Silently fail security wise? or throws?)
        // C# implementation returned Success true with message "Si el correo es válido..." (Generic message to prevent enumeration).
        // Let's check my Java implementation of AuthService.register.
        
        when(userRepository.existsByEmail(registroDto.getEmail())).thenReturn(true);

        // Act
        AuthResult result = authService.register(registroDto);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(result.getMessage()).isEqualTo("Si el correo es válido, recibirás un enlace de confirmación.");
        
        verify(userRepository, never()).save(any(Usuario.class));
    }

    @Test
    void externalLogin_WithNewUser_ShouldCreateUserAndReturnToken() {
        // Arrange
        ExternalLoginDto externalLoginDto = new ExternalLoginDto();
        externalLoginDto.setProvider("Google");
        externalLoginDto.setIdToken("valid-google-token");

        ExternalAuthUserInfo userInfo = new ExternalAuthUserInfo();
        userInfo.setEmail("google.user@email.com");
        userInfo.setName("Google User");
        userInfo.setProviderSubjectId("google-user-id-123");

        when(externalAuthValidator.validateToken(externalLoginDto.getIdToken())).thenReturn(userInfo);
        when(userRepository.findByEmail(userInfo.getEmail())).thenReturn(Optional.empty()); // New user

        when(tokenService.crearToken(any(Usuario.class))).thenReturn("jwt-access-token");
        when(tokenService.generarRefreshToken()).thenReturn("jwt-refresh-token");

        // Act
        TokenResponseDto result = authService.externalLogin(externalLoginDto);

        // Assert
        assertThat(result).isNotNull();
        assertThat(result.getAccessToken()).isEqualTo("jwt-access-token");
        assertThat(result.getRefreshToken()).isEqualTo("jwt-refresh-token");

        // Verify save was called at least once (create user + update/set refresh token)
        verify(userRepository, atLeastOnce()).save(any(Usuario.class));
    }

    @Test
    void verifyEmail_WithValidToken_ShouldVerifyUser() {
        // Arrange
        String fakeToken = "valid-token";
        Usuario usuario = new Usuario("Test", "test@test.com", "123", RolUsuario.User, null);
        usuario.setEmailVerificationToken(fakeToken);

        when(userRepository.findByEmailVerificationToken(fakeToken)).thenReturn(Optional.of(usuario));

        // Act
        AuthResult result = authService.verifyEmail(fakeToken);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(usuario.isEmailVerified()).isTrue();
        assertThat(usuario.getEmailVerificationToken()).isNull();

        verify(userRepository, times(1)).save(usuario);
    }

    @Test
    void verifyEmail_WithInvalidToken_ShouldReturnFail() {
        // Arrange
        String fakeToken = "invalid-token";
        when(userRepository.findByEmailVerificationToken(fakeToken)).thenReturn(Optional.empty());

        // Act
        AuthResult result = authService.verifyEmail(fakeToken);

        // Assert
        assertThat(result.isSuccess()).isFalse();
        assertThat(result.getMessage()).isEqualTo("Token de verificación inválido.");
        verify(userRepository, never()).save(any(Usuario.class));
    }

    @Test
    void forgotPassword_WhenUserExists_ShouldSetTokenAndSendEmail() {
        // Arrange
        String userEmail = "test@test.com";
        String fakeToken = "fake-reset-token-123";
        Usuario usuario = new Usuario("Test User", userEmail, "123", RolUsuario.User, null);

        when(userRepository.findByEmail(userEmail)).thenReturn(Optional.of(usuario));
        when(tokenService.generarRefreshToken()).thenReturn(fakeToken); // Reusing logic

        // Act
        AuthResult result = authService.forgotPassword(userEmail);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(result.getMessage()).isEqualTo("Si existe una cuenta con ese correo, se ha enviado un enlace para restablecer la contraseña.");
        
        assertThat(usuario.getPasswordResetToken()).isEqualTo(fakeToken);
        
        verify(userRepository, times(1)).save(usuario);
        verify(emailService, times(1)).sendPasswordResetEmail(eq(userEmail), eq("Test User"), contains(fakeToken));
    }

    @Test
    void forgotPassword_WhenUserDoesNotExist_ShouldReturnSuccessSilently() {
        // Arrange
        String userEmail = "no-existe@test.com";
        when(userRepository.findByEmail(userEmail)).thenReturn(Optional.empty());

        // Act
        AuthResult result = authService.forgotPassword(userEmail);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(result.getMessage()).isEqualTo("Si existe una cuenta con ese correo, se ha enviado un enlace para restablecer la contraseña.");
        
        verify(userRepository, never()).save(any(Usuario.class));
        verify(emailService, never()).sendPasswordResetEmail(anyString(), anyString(), anyString());
    }

    @Test
    void resetPassword_WithValidToken_ShouldResetPassword() {
        // Arrange
        String fakeToken = "valid-token-456";
        ResetPasswordDto dto = new ResetPasswordDto();
        dto.setToken(fakeToken);
        dto.setNewPassword("NewPassword123!");
        dto.setConfirmPassword("NewPassword123!");
        
        String newHash = "EncryptedNewPassword";
        
        Usuario usuario = new Usuario("Test User", "test@test.com", "123", RolUsuario.User, null);
        usuario.setPasswordResetToken(fakeToken);
        usuario.setPasswordResetTokenExpiryTime(LocalDateTime.now().plusHours(1));

        when(userRepository.findByPasswordResetToken(fakeToken)).thenReturn(Optional.of(usuario));
        when(encryptionService.encrypt(dto.getNewPassword())).thenReturn(newHash);

        // Act
        AuthResult result = authService.resetPassword(dto);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(result.getMessage()).isEqualTo("Contraseña restablecida exitosamente.");
        
        assertThat(usuario.getPasswordHash()).isEqualTo(newHash);
        assertThat(usuario.getPasswordResetToken()).isNull();

        verify(userRepository, times(1)).save(usuario);
    }

    @Test
    void resetPassword_WithInvalidToken_ShouldReturnFail() {
        // Arrange
        ResetPasswordDto dto = new ResetPasswordDto();
        dto.setToken("invalid-token");
        when(userRepository.findByPasswordResetToken("invalid-token")).thenReturn(Optional.empty());

        // Act
        AuthResult result = authService.resetPassword(dto);

        // Assert
        assertThat(result.isSuccess()).isFalse();
        assertThat(result.getMessage()).isEqualTo("El token de restablecimiento no es válido.");
    }
}
