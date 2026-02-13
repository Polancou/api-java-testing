package com.polancou.apibasecore.application.services;

import com.polancou.apibasecore.application.dtos.ActualizarPerfilDto;
import com.polancou.apibasecore.application.dtos.CambiarPasswordDto;
import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.application.interfaces.IEncryptionService;
import com.polancou.apibasecore.application.interfaces.IFileStorageService;
import com.polancou.apibasecore.application.interfaces.IProfileService;
import com.polancou.apibasecore.application.utilities.UserMapper;
import com.polancou.apibasecore.domain.models.AuthResult;
import com.polancou.apibasecore.domain.models.Usuario;
import com.polancou.apibasecore.infrastructure.repositories.UserRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.UUID;

@Service
public class ProfileService implements IProfileService {

    private final UserRepository userRepository;
    private final IFileStorageService fileStorageService;
    private final IEncryptionService encryptionService;

    public ProfileService(UserRepository userRepository, 
                          IFileStorageService fileStorageService, 
                          IEncryptionService encryptionService) {
        this.userRepository = userRepository;
        this.fileStorageService = fileStorageService;
        this.encryptionService = encryptionService;
    }

    @Override
    @Transactional(readOnly = true)
    public PerfilUsuarioDto getProfileById(UUID userId) {
        Usuario usuario = userRepository.findById(userId)
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.NotFoundException("Usuario no encontrado."));
        return UserMapper.toPerfilUsuarioDto(usuario);
    }

    @Override
    @Transactional
    public boolean actualizarPerfil(UUID userId, ActualizarPerfilDto perfilDto) {
        Usuario usuario = userRepository.findById(userId)
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.NotFoundException("Usuario no encontrado."));

        usuario.updateProfile(perfilDto.getName(), perfilDto.getPhone(), perfilDto.getTaxId());
        userRepository.save(usuario);
        return true;
    }

    @Override
    @Transactional
    public AuthResult cambiarPassword(UUID userId, CambiarPasswordDto dto) {
        Usuario usuario = userRepository.findById(userId).orElse(null);
        if (usuario == null) return AuthResult.fail("Usuario no encontrado.");

        if (usuario.getPasswordHash() == null) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("No puedes cambiar la contraseña de una cuenta de inicio de sesión externo.");
        }

        String decryptedCurrent = encryptionService.decrypt(usuario.getPasswordHash());
        if (!decryptedCurrent.equals(dto.getOldPassword())) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("La contraseña actual es incorrecta.");
        }

        String newEncrypted = encryptionService.encrypt(dto.getNewPassword());
        usuario.establecerPasswordHash(newEncrypted);
        userRepository.save(usuario);

        return AuthResult.ok(null, "Contraseña actualizada exitosamente.");
    }

    @Override
    @Transactional
    public String uploadAvatar(UUID userId, MultipartFile file) {
        Usuario usuario = userRepository.findById(userId)
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.NotFoundException("Usuario no encontrado."));

        if (usuario.getAvatarUrl() != null && !usuario.getAvatarUrl().isBlank()) {
            fileStorageService.deleteFile(usuario.getAvatarUrl());
        }

        String uniqueFileName = UUID.randomUUID().toString() + "_" + file.getOriginalFilename();
        String fileUrl;
        try {
            fileUrl = fileStorageService.saveFile(file.getInputStream(), uniqueFileName);
        } catch (IOException e) {
            throw new RuntimeException("Error uploading file", e);
        }

        usuario.setAvatarUrl(fileUrl);
        userRepository.save(usuario);

        return fileUrl;
    }
}
