package com.polancou.apibasecore.application.services;

import com.polancou.apibasecore.application.dtos.PerfilUsuarioDto;
import com.polancou.apibasecore.application.dtos.RegistroUsuarioDto;
import com.polancou.apibasecore.application.dtos.UpdateUserDto;
import com.polancou.apibasecore.application.dtos.UserFilterDto;
import com.polancou.apibasecore.application.interfaces.IEncryptionService;
import com.polancou.apibasecore.application.interfaces.IUserService;
import com.polancou.apibasecore.application.utilities.UserMapper;
import com.polancou.apibasecore.application.utilities.ValidationUtilities;
import com.polancou.apibasecore.domain.enums.RolUsuario;
import com.polancou.apibasecore.domain.models.Usuario;
import com.polancou.apibasecore.infrastructure.repositories.UserRepository;
import com.polancou.apibasecore.infrastructure.repositories.UserSpecification;
import org.springframework.data.domain.Sort;
import org.springframework.data.jpa.domain.Specification;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
public class UserService implements IUserService {

    private final UserRepository userRepository;
    private final IEncryptionService encryptionService;

    public UserService(UserRepository userRepository, IEncryptionService encryptionService) {
        this.userRepository = userRepository;
        this.encryptionService = encryptionService;
    }

    @Override
    @Transactional(readOnly = true)
    public List<PerfilUsuarioDto> getUsers(UserFilterDto filterDto) {
        Specification<Usuario> spec = Specification.where((Specification<Usuario>) null);

        if (filterDto.getFilter() != null && !filterDto.getFilter().isBlank()) {
            spec = spec.and(UserSpecification.filterBy(filterDto.getFilter()));
        }

        Sort sort = Sort.unsorted();
        if (filterDto.getSortedBy() != null && !filterDto.getSortedBy().isBlank()) {
            String sortBy = filterDto.getSortedBy().toLowerCase();
            switch (sortBy) {
                case "email": sort = Sort.by("email"); break;
                case "id": sort = Sort.by("id"); break;
                case "name": sort = Sort.by("name"); break;
                case "phone": sort = Sort.by("phone"); break;
                case "tax_id": sort = Sort.by("taxId"); break;
                case "created_at": sort = Sort.by("createdAt"); break;
            }
        }

        return userRepository.findAll(spec, sort).stream()
                .map(UserMapper::toPerfilUsuarioDto)
                .collect(Collectors.toList());
    }

    @Override
    @Transactional(readOnly = true)
    public PerfilUsuarioDto getUserById(UUID id) {
        Usuario user = userRepository.findById(id)
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.NotFoundException("Usuario no encontrado."));
        return UserMapper.toPerfilUsuarioDto(user);
    }

    @Override
    @Transactional
    public PerfilUsuarioDto createUser(RegistroUsuarioDto createUserDto) {
        // Validation
        if (createUserDto.getTaxId() != null && !createUserDto.getTaxId().isBlank()) {
            if (!ValidationUtilities.validateTaxIdRFC(createUserDto.getTaxId())) {
                throw new com.polancou.apibasecore.application.exceptions.ValidationException("El formato del Tax ID (RFC) es inválido.");
            }
            if (userRepository.existsByTaxId(createUserDto.getTaxId())) {
                throw new com.polancou.apibasecore.application.exceptions.ValidationException("El Tax ID ya está registrado.");
            }
        }

        if (!ValidationUtilities.validatePhoneAndresFormat(createUserDto.getPhone())) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("El formato del teléfono es inválido (AndresFormat). Debe ser de 10 dígitos.");
        }

        if (userRepository.existsByEmail(createUserDto.getEmail())) {
            throw new com.polancou.apibasecore.application.exceptions.ValidationException("El email ya está registrado.");
        }

        String encryptedPassword = encryptionService.encrypt(createUserDto.getPassword());

        Usuario newUser = new Usuario(
                createUserDto.getName(),
                createUserDto.getEmail(),
                createUserDto.getPhone(),
                RolUsuario.User,
                createUserDto.getTaxId()
        );
        newUser.establecerPasswordHash(encryptedPassword);

        userRepository.save(newUser);

        return UserMapper.toPerfilUsuarioDto(newUser);
    }

    @Override
    @Transactional
    public PerfilUsuarioDto updateUser(UUID id, UpdateUserDto updateDto) {
        Usuario user = userRepository.findById(id)
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.NotFoundException("Usuario no encontrado."));

        if (updateDto.getEmail() != null) {
            // Check uniqueness excluding self
             userRepository.findByEmail(updateDto.getEmail())
                     .ifPresent(u -> {
                         if (!u.getId().equals(id)) throw new com.polancou.apibasecore.application.exceptions.ValidationException("El email ya existe.");
                     });
        }

        if (updateDto.getTaxId() != null) {
            if (!ValidationUtilities.validateTaxIdRFC(updateDto.getTaxId())) {
                throw new com.polancou.apibasecore.application.exceptions.ValidationException("Invalid Tax ID format.");
            }
            userRepository.findByTaxId(updateDto.getTaxId())
                    .ifPresent(u -> {
                        if (!u.getId().equals(id)) throw new com.polancou.apibasecore.application.exceptions.ValidationException("Tax ID already in use.");
                    });
        }

        if (updateDto.getPhone() != null) {
             if (!ValidationUtilities.validatePhoneAndresFormat(updateDto.getPhone())) {
                 throw new com.polancou.apibasecore.application.exceptions.ValidationException("Invalid Phone format.");
             }
        }

        user.updateProfile(
                updateDto.getName(), 
                updateDto.getPhone(),
                updateDto.getTaxId()
        );

        if (updateDto.getPassword() != null && !updateDto.getPassword().isBlank()) {
            String encrypted = encryptionService.encrypt(updateDto.getPassword());
            user.establecerPasswordHash(encrypted);
        }

        userRepository.save(user);

        return UserMapper.toPerfilUsuarioDto(user);
    }

    @Override
    @Transactional
    public void deleteUser(UUID id) {
        Usuario user = userRepository.findById(id)
                .orElseThrow(() -> new com.polancou.apibasecore.application.exceptions.NotFoundException("Usuario no encontrado."));
        userRepository.delete(user);
    }
}
