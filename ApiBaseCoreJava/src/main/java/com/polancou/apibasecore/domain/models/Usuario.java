package com.polancou.apibasecore.domain.models;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.polancou.apibasecore.domain.enums.RolUsuario;
import jakarta.persistence.*;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.NoArgsConstructor;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Entity
@Table(name = "Usuarios", indexes = {
    @Index(name = "IX_Usuarios_TaxId", columnList = "TaxId", unique = true)
})
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class Usuario {

    @Id
    @JsonProperty("id")
    private UUID id;

    @Column(nullable = false, length = 100)
    @JsonProperty("email")
    private String email;

    @Column(nullable = false, length = 100)
    @JsonProperty("name")
    private String name;

    @Column(nullable = false, length = 20)
    @JsonProperty("phone")
    private String phone;

    @JsonIgnore
    @Column(name = "PasswordHash")
    private String passwordHash;

    @Column(length = 20, unique = true)
    @JsonProperty("tax_id")
    private String taxId;

    @JsonProperty("created_at")
    private LocalDateTime createdAt;

    @Enumerated(EnumType.ORDINAL) // Matching C# default (int)
    @Column(nullable = false)
    @JsonIgnore // In C# it was JsonIgnore, but usually we want role in response? C# code had [JsonIgnore]
    private RolUsuario rol;

    @OneToMany(mappedBy = "usuario", cascade = CascadeType.ALL, orphanRemoval = true)
    @JsonProperty("addresses")
    private List<Address> addresses = new ArrayList<>();

    // --- Technical/Auth Fields ---

    @JsonIgnore
    private String avatarUrl;

    @JsonIgnore
    private String refreshToken;

    @JsonIgnore
    private LocalDateTime refreshTokenExpiryTime;

    @Version
    @JsonIgnore
    private Long rowVersion; // Hibernate uses Long or Integer for versioning, check compatibility with byte[]

    @JsonIgnore
    private String emailVerificationToken;

    @JsonIgnore
    private boolean isEmailVerified;

    @JsonIgnore
    private String passwordResetToken;

    @JsonIgnore
    private LocalDateTime passwordResetTokenExpiryTime;

    // Constructors

    public Usuario(String name, String email, String phone, RolUsuario rol, String taxId) {
        if (name == null || name.isBlank()) throw new IllegalArgumentException("El nombre es obligatorio.");
        if (email == null || email.isBlank()) throw new IllegalArgumentException("El email es obligatorio.");

        this.id = UUID.randomUUID();
        this.name = name;
        this.email = email;
        this.phone = phone;
        this.rol = rol;
        this.taxId = taxId;
        this.createdAt = LocalDateTime.now(ZoneId.of("UTC")); // Similar to DateTime.UtcNow
        this.isEmailVerified = false;
    }

    // Methods
    public void setId(UUID id) {
        this.id = id;
    }

    public void establecerPasswordHash(String passwordHash) {
        if (passwordHash == null || passwordHash.isBlank())
            throw new IllegalArgumentException("El hash de la contraseña no puede estar vacío.");
        this.passwordHash = passwordHash;
    }

    public void updateProfile(String name, String phone, String taxId) {
        if (name != null && !name.isBlank()) this.name = name;
        if (phone != null && !phone.isBlank()) this.phone = phone;
        if (taxId != null) this.taxId = taxId;
    }

    public void addAddress(Address address) {
        addresses.add(address);
        address.setUsuario(this);
    }

    public void removeAddress(Integer addressId) {
        addresses.removeIf(a -> a.getId().equals(addressId));
    }

    public void setAvatarUrl(String nuevoUrl) {
        if (nuevoUrl != null && !nuevoUrl.isBlank()) this.avatarUrl = nuevoUrl;
    }

    public void setRefreshToken(String refreshToken, LocalDateTime expiryTime) {
        this.refreshToken = refreshToken;
        this.refreshTokenExpiryTime = expiryTime;
    }

    public void setRol(RolUsuario rol) {
        this.rol = rol;
    }

    public void setEmailVerificationToken(String token) {
        this.emailVerificationToken = token;
    }

    public void markEmailAsVerified() {
        this.isEmailVerified = true;
        this.emailVerificationToken = null;
    }

    public void setPasswordResetToken(String token, LocalDateTime expiryTime) {
        this.passwordResetToken = token;
        this.passwordResetTokenExpiryTime = expiryTime;
    }

    public void clearPasswordResetToken() {
        this.passwordResetToken = null;
        this.passwordResetTokenExpiryTime = null;
    }
}
