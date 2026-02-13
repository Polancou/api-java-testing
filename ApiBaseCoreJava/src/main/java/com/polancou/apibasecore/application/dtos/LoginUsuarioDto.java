package com.polancou.apibasecore.application.dtos;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class LoginUsuarioDto {
    @NotBlank
    private String email;
    
    @NotBlank
    private String password;
}
