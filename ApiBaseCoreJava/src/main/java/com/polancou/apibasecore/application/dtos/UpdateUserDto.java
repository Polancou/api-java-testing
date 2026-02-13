package com.polancou.apibasecore.application.dtos;

import jakarta.validation.constraints.Email;
import lombok.Data;

@Data
public class UpdateUserDto {
    @Email
    private String email;
    
    private String name;
    
    private String phone;
    
    private String taxId;
    
    private String password;
}
