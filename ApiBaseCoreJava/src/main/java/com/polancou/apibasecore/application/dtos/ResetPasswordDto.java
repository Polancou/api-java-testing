package com.polancou.apibasecore.application.dtos;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class ResetPasswordDto {
    @NotBlank
    private String token;
    
    @NotBlank
    private String newPassword;
    
    @NotBlank
    private String confirmPassword;
}
