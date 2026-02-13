package com.polancou.apibasecore.api.controllers;

import com.polancou.apibasecore.application.dtos.*;
import com.polancou.apibasecore.application.interfaces.IAuthService;
import com.polancou.apibasecore.domain.models.AuthResult;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletResponse;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.Duration;
import java.util.Collections;
import java.util.Map;

@RestController
@RequestMapping("/api/v1/auth")
public class AuthController {

    private final IAuthService authService;

    public AuthController(IAuthService authService) {
        this.authService = authService;
    }

    @PostMapping("/register")
    public ResponseEntity<Map<String, String>> register(@Valid @RequestBody RegistroUsuarioDto registroDto) {
        AuthResult result = authService.register(registroDto);
        if (!result.isSuccess()) {
            return ResponseEntity.badRequest().body(Collections.singletonMap("message", result.getMessage()));
        }
        return ResponseEntity.ok(Collections.singletonMap("message", result.getMessage()));
    }

    @PostMapping("/login")
    public ResponseEntity<Map<String, String>> login(@Valid @RequestBody LoginUsuarioDto loginDto, HttpServletResponse response) {
        TokenResponseDto tokenResponse = authService.login(loginDto);
        setRefreshTokenInCookie(response, tokenResponse.getRefreshToken());
        return ResponseEntity.ok(Collections.singletonMap("accessToken", tokenResponse.getAccessToken()));
    }

    @PostMapping("/external-login")
    public ResponseEntity<Map<String, String>> externalLogin(@Valid @RequestBody ExternalLoginDto externalLoginDto, HttpServletResponse response) {
        TokenResponseDto tokenResponse = authService.externalLogin(externalLoginDto);
        setRefreshTokenInCookie(response, tokenResponse.getRefreshToken());
        return ResponseEntity.ok(Collections.singletonMap("accessToken", tokenResponse.getAccessToken()));
    }

    @PostMapping("/refresh")
    public ResponseEntity<Map<String, String>> refresh(@CookieValue(name = "refreshToken", defaultValue = "") String refreshToken, HttpServletResponse response) {
        if (refreshToken.isEmpty()) {
            return ResponseEntity.badRequest().body(Collections.singletonMap("message", "No se encontró el token de refresco en las cookies."));
        }

        TokenResponseDto tokenResponse = authService.refreshToken(refreshToken);
        setRefreshTokenInCookie(response, tokenResponse.getRefreshToken());
        return ResponseEntity.ok(Collections.singletonMap("accessToken", tokenResponse.getAccessToken()));
    }

    @PostMapping("/logout")
    public ResponseEntity<Map<String, String>> logout(HttpServletResponse response) {
        Cookie cookie = new Cookie("refreshToken", null);
        cookie.setHttpOnly(true);
        cookie.setSecure(true); // Should depend on request.isSecure() but enforced here for safety
        cookie.setPath("/");
        cookie.setMaxAge(0);
        response.addCookie(cookie);
        return ResponseEntity.ok(Collections.singletonMap("message", "Sesión cerrada correctamente."));
    }

    @GetMapping("/verify-email")
    public ResponseEntity<Map<String, String>> verifyEmail(@RequestParam String token) {
        AuthResult result = authService.verifyEmail(token);
        if (!result.isSuccess()) {
            return ResponseEntity.badRequest().body(Collections.singletonMap("message", result.getMessage()));
        }
        return ResponseEntity.ok(Collections.singletonMap("message", result.getMessage()));
    }

    @PostMapping("/forgot-password")
    public ResponseEntity<Map<String, String>> forgotPassword(@Valid @RequestBody Map<String, String> body) {
        String email = body.get("email"); // Simplified DTO
        if (email == null) return ResponseEntity.badRequest().build();
        
        AuthResult result = authService.forgotPassword(email);
        if (!result.isSuccess()) {
            return ResponseEntity.badRequest().body(Collections.singletonMap("message", result.getMessage()));
        }
        return ResponseEntity.ok(Collections.singletonMap("message", result.getMessage()));
    }

    @PostMapping("/reset-password")
    public ResponseEntity<Map<String, String>> resetPassword(@Valid @RequestBody ResetPasswordDto dto) {
        AuthResult result = authService.resetPassword(dto);
        if (!result.isSuccess()) {
            return ResponseEntity.badRequest().body(Collections.singletonMap("message", result.getMessage()));
        }
        return ResponseEntity.ok(Collections.singletonMap("message", result.getMessage()));
    }

    private void setRefreshTokenInCookie(HttpServletResponse response, String refreshToken) {
        Cookie cookie = new Cookie("refreshToken", refreshToken);
        cookie.setHttpOnly(true);
        cookie.setSecure(true); // Typically depends on env, but assuming HTTPS for production-like
        // SameSite is not strictly supported by Servlet Cookie API directly in older versions, 
        // but Spring Boot 3 / Servlet 6.0 might support setAttribute.
        cookie.setPath("/");
        cookie.setMaxAge((int) Duration.ofDays(30).toSeconds());
        cookie.setAttribute("SameSite", "Lax"); // Servlet 6.0 feature
        response.addCookie(cookie);
    }
}
