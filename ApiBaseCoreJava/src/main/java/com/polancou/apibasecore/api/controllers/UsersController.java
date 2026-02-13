package com.polancou.apibasecore.api.controllers;

import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.application.dtos.RegistroUsuarioDto;
import com.polancou.apibasecore.application.dtos.UpdateUserDto;
import com.polancou.apibasecore.application.dtos.UserFilterDto;
import com.polancou.apibasecore.application.interfaces.IUserService;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.net.URI;
import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/users")
public class UsersController {

    private final IUserService userService;

    public UsersController(IUserService userService) {
        this.userService = userService;
    }

    @GetMapping
    public ResponseEntity<List<PerfilUsuarioDto>> getUsers(@ModelAttribute UserFilterDto filterDto) {
        return ResponseEntity.ok(userService.getUsers(filterDto));
    }

    @PostMapping
    public ResponseEntity<PerfilUsuarioDto> createUser(@Valid @RequestBody RegistroUsuarioDto createUserDto) {
        PerfilUsuarioDto createdUser = userService.createUser(createUserDto);
        URI location = ServletUriComponentsBuilder
                .fromCurrentRequest()
                .path("/{id}")
                .buildAndExpand(createdUser.getId())
                .toUri();
        return ResponseEntity.created(location).body(createdUser);
    }

    @GetMapping("/{id}")
    public ResponseEntity<PerfilUsuarioDto> getUserById(@PathVariable UUID id) {
        return ResponseEntity.ok(userService.getUserById(id));
    }

    @PatchMapping("/{id}")
    public ResponseEntity<PerfilUsuarioDto> updateUser(@PathVariable UUID id, @Valid @RequestBody UpdateUserDto updateDto) {
        return ResponseEntity.ok(userService.updateUser(id, updateDto));
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteUser(@PathVariable UUID id) {
        userService.deleteUser(id);
        return ResponseEntity.noContent().build();
    }
}
