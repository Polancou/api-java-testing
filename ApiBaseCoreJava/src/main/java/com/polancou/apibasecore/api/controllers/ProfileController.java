package com.polancou.apibasecore.api.controllers;

import com.polancou.apibasecore.application.dtos.ActualizarPerfilDto;
import com.polancou.apibasecore.application.dtos.CambiarPasswordDto;
import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.application.interfaces.IProfileService;
import com.polancou.apibasecore.domain.models.AuthResult;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.util.Collections;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/profile")
public class ProfileController {

    private final IProfileService profileService;

    public ProfileController(IProfileService profileService) {
        this.profileService = profileService;
    }

    private UUID getCurrentUserId() {
        Authentication authentication = SecurityContextHolder.getContext().getAuthentication();
        if (authentication == null || !authentication.isAuthenticated()) {
            throw new RuntimeException("User not authenticated");
        }
        // In JwtAuthenticationFilter we set principal name as userID
        return UUID.fromString(authentication.getName());
    }

    @GetMapping("/me")
    public ResponseEntity<PerfilUsuarioDto> getMyProfile() {
        return ResponseEntity.ok(profileService.getProfileById(getCurrentUserId()));
    }

    @PutMapping("/me")
    public ResponseEntity<Void> updateMyProfile(@Valid @RequestBody ActualizarPerfilDto perfilDto) {
        profileService.actualizarPerfil(getCurrentUserId(), perfilDto);
        return ResponseEntity.noContent().build();
    }

    @PutMapping("/change-password")
    public ResponseEntity<Map<String, String>> changePassword(@Valid @RequestBody CambiarPasswordDto dto) {
        AuthResult result = profileService.cambiarPassword(getCurrentUserId(), dto);
        if (!result.isSuccess()) {
            return ResponseEntity.badRequest().body(Collections.singletonMap("message", result.getMessage()));
        }
        return ResponseEntity.ok(Collections.singletonMap("message", result.getMessage()));
    }

    @PostMapping("/avatar")
    public ResponseEntity<Map<String, String>> uploadAvatar(@RequestParam("file") MultipartFile file) {
        if (file.isEmpty()) {
             return ResponseEntity.badRequest().body(Collections.singletonMap("message", "No se ha proporcionado ningún archivo."));
        }
        if (file.getSize() > 2 * 1024 * 1024) {
             return ResponseEntity.badRequest().body(Collections.singletonMap("message", "El archivo debe ser menor a 2 MB."));
        }
        
        // TODO: Validate file signature (magic numbers) as in C# FileSignatureValidator. 
        // Skipping for now to keep it simple, but noted.
        
        String newAvatarUrl = profileService.uploadAvatar(getCurrentUserId(), file);
        return ResponseEntity.ok(Collections.singletonMap("avatarUrl", newAvatarUrl));
    }
}
