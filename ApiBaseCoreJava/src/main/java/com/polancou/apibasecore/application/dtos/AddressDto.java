package com.polancou.apibasecore.application.dtos;

import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.Data;

@Data
public class AddressDto {
    private int id;
    private String name;
    private String street;
    
    @JsonProperty("country_code")
    private String countryCode;
}
