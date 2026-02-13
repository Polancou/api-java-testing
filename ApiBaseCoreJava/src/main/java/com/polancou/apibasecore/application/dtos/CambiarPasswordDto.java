package com.polancou.apibasecore.application.dtos;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class CambiarPasswordDto {
    @NotBlank
    private String oldPassword;
    
    @NotBlank
    private String newPassword;
    
    @NotBlank
    private String confirmPassword;
}
