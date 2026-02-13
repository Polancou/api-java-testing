package com.polancou.apibasecore.application.dtos;

import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.Data;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Data
public class PerfilUsuarioDto {
    private UUID id;
    private String name;
    private String email;
    private String phone;
    
    @JsonProperty("tax_id")
    private String taxId;
    
    @JsonProperty("created_at")
    private String createdAt;
    
    private String avatarUrl;
    private String rol;
    
    private List<AddressDto> addresses = new ArrayList<>();
}
