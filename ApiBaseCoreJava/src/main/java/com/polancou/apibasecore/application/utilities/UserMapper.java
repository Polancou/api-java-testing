package com.polancou.apibasecore.application.utilities;

import com.polancou.apibasecore.application.dtos.AddressDto;
import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.domain.models.Address;
import com.polancou.apibasecore.domain.models.Usuario;

import java.util.stream.Collectors;

public class UserMapper {

    public static PerfilUsuarioDto toPerfilUsuarioDto(Usuario usuario) {
        if (usuario == null) return null;

        PerfilUsuarioDto dto = new PerfilUsuarioDto();
        dto.setId(usuario.getId());
        dto.setName(usuario.getName());
        dto.setEmail(usuario.getEmail());
        dto.setPhone(usuario.getPhone());
        dto.setTaxId(usuario.getTaxId());
        dto.setCreatedAt(usuario.getCreatedAt().toString()); // ISO 8601
        dto.setAvatarUrl(usuario.getAvatarUrl());
        dto.setRol(usuario.getRol().toString());

        if (usuario.getAddresses() != null) {
            dto.setAddresses(usuario.getAddresses().stream()
                    .map(UserMapper::toAddressDto)
                    .collect(Collectors.toList()));
        }

        return dto;
    }

    public static AddressDto toAddressDto(Address address) {
        if (address == null) return null;
        AddressDto dto = new AddressDto();
        dto.setId(address.getId());
        dto.setName(address.getName());
        dto.setStreet(address.getStreet());
        dto.setCountryCode(address.getCountryCode());
        return dto;
    }
}
