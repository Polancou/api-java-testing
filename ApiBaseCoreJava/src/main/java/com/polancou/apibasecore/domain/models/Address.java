package com.polancou.apibasecore.domain.models;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import lombok.*;

@Entity
@Table(name = "Addresses")
@Getter
@NoArgsConstructor(access = AccessLevel.PROTECTED)
public class Address {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(nullable = false, length = 100)
    @JsonProperty("name")
    private String name;

    @Column(nullable = false, length = 200)
    @JsonProperty("street")
    private String street;

    @Column(nullable = false, length = 2)
    @JsonProperty("country_code")
    private String countryCode;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "UsuarioId", nullable = false)
    @JsonIgnore
    private Usuario usuario;

    public Address(String name, String street, String countryCode) {
        this.name = name;
        this.street = street;
        this.countryCode = countryCode;
    }
    
    // Setter for Usuario to be used when adding address to user
    protected void setUsuario(Usuario usuario) {
        this.usuario = usuario;
    }
}
