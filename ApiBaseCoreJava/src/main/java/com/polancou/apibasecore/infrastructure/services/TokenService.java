package com.polancou.apibasecore.infrastructure.services;

import com.polancou.apibasecore.application.interfaces.ITokenService;
import com.polancou.apibasecore.domain.models.Usuario;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.io.Decoders;
import io.jsonwebtoken.security.Keys;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.security.SecureRandom;
import java.util.Base64;
import java.util.Date;
import java.util.UUID;

@Service
public class TokenService implements ITokenService {

    private final String key;
    private final String issuer;

    public TokenService(@Value("${jwt.key}") String key,
                        @Value("${jwt.issuer}") String issuer) {
        this.key = key;
        this.issuer = issuer;
    }

    @Override
    public String crearToken(Usuario usuario) {
        
        SecretKey secretKey = Keys.hmacShaKeyFor(key.getBytes(StandardCharsets.UTF_8));
        
        // In C# "sub" maps to NameIdentifier, "role" to Role. 
        // JJWT standard uses "sub" for subject.
        
        return Jwts.builder()
                .subject(usuario.getId().toString())
                .claim("email", usuario.getEmail())
                .claim("role", usuario.getRol().toString()) // Enum to String
                .id(UUID.randomUUID().toString()) // jti
                .issuer(issuer)
                .issuedAt(new Date())
                .expiration(new Date(System.currentTimeMillis() + 15 * 60 * 1000)) // 15 minutes
                .signWith(secretKey)
                .compact();
    }

    @Override
    public String generarRefreshToken() {
        byte[] randomNumber = new byte[64];
        new SecureRandom().nextBytes(randomNumber);
        return Base64.getEncoder().encodeToString(randomNumber);
    }
}
