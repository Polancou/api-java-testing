package com.polancou.apibasecore.application.interfaces;

import com.polancou.apibasecore.application.dtos.ActualizarPerfilDto;
import com.polancou.apibasecore.application.dtos.CambiarPasswordDto;
import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.domain.models.AuthResult;
import org.springframework.web.multipart.MultipartFile;

import java.util.UUID;

public interface IProfileService {
    PerfilUsuarioDto getProfileById(UUID userId);
    boolean actualizarPerfil(UUID userId, ActualizarPerfilDto perfilDto);
    AuthResult cambiarPassword(UUID userId, CambiarPasswordDto dto);
    String uploadAvatar(UUID userId, MultipartFile file);
}
