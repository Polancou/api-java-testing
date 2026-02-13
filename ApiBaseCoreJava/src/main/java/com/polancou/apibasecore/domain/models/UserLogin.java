package com.polancou.apibasecore.domain.models;

import jakarta.persistence.*;
import lombok.*;
import java.io.Serializable;

// Composite Key Class
@Data
@NoArgsConstructor
@AllArgsConstructor
class UserLoginId implements Serializable {
    private String loginProvider;
    private String providerKey;
}

@Entity
@Table(name = "UserLogins")
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
@IdClass(UserLoginId.class)
public class UserLogin {

    @Id
    @Column(nullable = false)
    private String loginProvider;

    @Id
    @Column(nullable = false)
    private String providerKey;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "UsuarioId", nullable = false)
    private Usuario usuario;

    public UserLogin(String loginProvider, String providerKey, Usuario usuario) {
        if (loginProvider == null || loginProvider.isBlank()) throw new IllegalArgumentException("LoginProvider cannot be null or empty");
        if (providerKey == null || providerKey.isBlank()) throw new IllegalArgumentException("ProviderKey cannot be null or empty");
        if (usuario == null) throw new IllegalArgumentException("Usuario cannot be null");

        this.loginProvider = loginProvider;
        this.providerKey = providerKey;
        this.usuario = usuario;
    }
}
