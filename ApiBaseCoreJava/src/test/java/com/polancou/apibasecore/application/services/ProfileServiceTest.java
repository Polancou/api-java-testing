package com.polancou.apibasecore.application.services;

import com.polancou.apibasecore.application.dtos.*;
import com.polancou.apibasecore.application.exceptions.NotFoundException;
import com.polancou.apibasecore.application.exceptions.ValidationException;
import com.polancou.apibasecore.application.interfaces.IEncryptionService;
import com.polancou.apibasecore.application.interfaces.IFileStorageService;
import com.polancou.apibasecore.domain.enums.RolUsuario;
import com.polancou.apibasecore.domain.models.AuthResult;
import com.polancou.apibasecore.domain.models.Usuario;
import com.polancou.apibasecore.infrastructure.repositories.UserRepository;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.io.InputStream;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class ProfileServiceTest {

    @Mock
    private UserRepository userRepository;
    @Mock
    private IFileStorageService fileStorageService;
    @Mock
    private IEncryptionService encryptionService;

    @InjectMocks
    private ProfileService profileService;

    @Test
    void getProfileById_WhenUserExists_ShouldReturnProfileDto() {
        // Arrange
        UUID userId = UUID.randomUUID();
        Usuario usuario = new Usuario("Usuario de Prueba", "test@email.com", "123", RolUsuario.User, null);
        usuario.setId(userId); // Assuming public setter or using reflection in real scenario just like C# test used logic

        when(userRepository.findById(userId)).thenReturn(Optional.of(usuario));

        // Act
        PerfilUsuarioDto result = profileService.getProfileById(userId);

        // Assert
        assertThat(result).isNotNull();
        assertThat(result.getId()).isEqualTo(userId);
        assertThat(result.getName()).isEqualTo("Usuario de Prueba");
    }

    @Test
    void getProfileById_WhenUserDoesNotExist_ShouldThrowNotFoundException() {
        // Arrange
        UUID userId = UUID.randomUUID();
        when(userRepository.findById(userId)).thenReturn(Optional.empty());

        // Act & Assert
        assertThrows(NotFoundException.class, () -> profileService.getProfileById(userId));
    }

    @Test
    void actualizarPerfil_WhenUserExists_ShouldUpdateAndSaveChanges() {
        // Arrange
        UUID userId = UUID.randomUUID();
        Usuario usuario = new Usuario("Nombre Original", "test@email.com", "123", RolUsuario.User, null);
        
        ActualizarPerfilDto updateDto = new ActualizarPerfilDto();
        updateDto.setName("Nombre Actualizado");
        updateDto.setPhone("9876543210");

        when(userRepository.findById(userId)).thenReturn(Optional.of(usuario));

        // Act
        boolean result = profileService.actualizarPerfil(userId, updateDto);

        // Assert
        assertThat(result).isTrue();
        assertThat(usuario.getName()).isEqualTo("Nombre Actualizado");
        assertThat(usuario.getPhone()).isEqualTo("9876543210");
        
        verify(userRepository, times(1)).save(usuario);
    }

    @Test
    void actualizarPerfil_WhenUserDoesNotExist_ShouldThrowNotFoundException() {
        // Arrange
        UUID userId = UUID.randomUUID();
        ActualizarPerfilDto updateDto = new ActualizarPerfilDto();
        when(userRepository.findById(userId)).thenReturn(Optional.empty());

        // Act & Assert
        assertThrows(NotFoundException.class, () -> profileService.actualizarPerfil(userId, updateDto));
        verify(userRepository, never()).save(any(Usuario.class));
    }

    @Test
    void cambiarPassword_WhenOldPasswordIsCorrect_ShouldUpdatePassword() {
        // Arrange
        UUID userId = UUID.randomUUID();
        String oldPassword = "PasswordAntigua123!";
        String newPassword = "PasswordNueva456!";
        
        Usuario usuario = new Usuario("Test User", "test@email.com", "123", RolUsuario.User, null);
        String oldHash = "EncryptedOld";
        String newHash = "EncryptedNew";
        
        usuario.establecerPasswordHash(oldHash);

        when(userRepository.findById(userId)).thenReturn(Optional.of(usuario));
        when(encryptionService.decrypt(oldHash)).thenReturn(oldPassword);
        when(encryptionService.encrypt(newPassword)).thenReturn(newHash);

        CambiarPasswordDto dto = new CambiarPasswordDto();
        dto.setOldPassword(oldPassword);
        dto.setNewPassword(newPassword);
        dto.setConfirmPassword(newPassword);

        // Act
        AuthResult result = profileService.cambiarPassword(userId, dto);

        // Assert
        assertThat(result.isSuccess()).isTrue();
        assertThat(usuario.getPasswordHash()).isEqualTo(newHash);
        verify(userRepository, times(1)).save(usuario);
    }

    @Test
    void cambiarPassword_WhenOldPasswordIsIncorrect_ShouldThrowValidationException() {
        // Arrange
        UUID userId = UUID.randomUUID();
        String correctOldPassword = "PasswordAntigua123!";
        String wrongOldPassword = "PasswordEquivocadaXXX";
        
        Usuario usuario = new Usuario("Test User", "test@email.com", "123", RolUsuario.User, null);
        String oldHash = "EncryptedOld";
        usuario.establecerPasswordHash(oldHash);

        when(userRepository.findById(userId)).thenReturn(Optional.of(usuario));
        when(encryptionService.decrypt(oldHash)).thenReturn(correctOldPassword);

        CambiarPasswordDto dto = new CambiarPasswordDto();
        dto.setOldPassword(wrongOldPassword);
        dto.setNewPassword("New123!");

        // Act & Assert
        assertThrows(ValidationException.class, () -> profileService.cambiarPassword(userId, dto));
        verify(userRepository, never()).save(any(Usuario.class));
    }

    @Test
    void uploadAvatar_WhenUserExists_ShouldSaveFileAndUpdateUser() throws IOException {
        // Arrange
        UUID userId = UUID.randomUUID();
        String fakeFileUrl = "/uploads/avatars/fake-guid.jpg";
        Usuario usuario = new Usuario("Test User", "test@email.com", "123", RolUsuario.User, null);

        MultipartFile mockFile = mock(MultipartFile.class);
        when(mockFile.getOriginalFilename()).thenReturn("test.jpg");
        when(mockFile.getInputStream()).thenReturn(mock(InputStream.class));

        when(userRepository.findById(userId)).thenReturn(Optional.of(usuario));
        when(fileStorageService.saveFile(any(InputStream.class), anyString())).thenReturn(fakeFileUrl);

        // Act
        String resultUrl = profileService.uploadAvatar(userId, mockFile);

        // Assert
        assertThat(resultUrl).isEqualTo(fakeFileUrl);
        assertThat(usuario.getAvatarUrl()).isEqualTo(fakeFileUrl);
        
        verify(fileStorageService, times(1)).saveFile(any(InputStream.class), anyString());
        verify(userRepository, times(1)).save(usuario);
    }
}
