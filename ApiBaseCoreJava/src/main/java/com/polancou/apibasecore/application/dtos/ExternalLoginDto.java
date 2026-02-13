package com.polancou.apibasecore.application.dtos;

import lombok.Data;

@Data
public class ExternalLoginDto {
    private String provider;
    private String idToken;
}
