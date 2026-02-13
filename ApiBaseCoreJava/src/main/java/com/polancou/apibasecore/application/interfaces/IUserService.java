package com.polancou.apibasecore.application.interfaces;

import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.application.dtos.RegistroUsuarioDto;
import com.polancou.apibasecore.application.dtos.UpdateUserDto;
import com.polancou.apibasecore.application.dtos.UserFilterDto;

import java.util.List;
import java.util.UUID;

public interface IUserService {
    List<PerfilUsuarioDto> getUsers(UserFilterDto filterDto);
    PerfilUsuarioDto getUserById(UUID id);
    PerfilUsuarioDto createUser(RegistroUsuarioDto createUserDto);
    PerfilUsuarioDto updateUser(UUID id, UpdateUserDto updateDto);
    void deleteUser(UUID id);
}
