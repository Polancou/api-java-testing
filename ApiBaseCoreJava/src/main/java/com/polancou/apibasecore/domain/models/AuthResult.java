package com.polancou.apibasecore.domain.models;

import lombok.AccessLevel;
import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor(access = AccessLevel.PRIVATE)
public class AuthResult {
    
    private final boolean success;
    private final String message;
    private final String token;

    public static AuthResult ok(String token, String message) {
        return new AuthResult(true, message, token);
    }
    
    public static AuthResult ok(String token) {
        return new AuthResult(true, "Operación exitosa.", token);
    }

    public static AuthResult fail(String message) {
        return new AuthResult(false, message, null);
    }
}
