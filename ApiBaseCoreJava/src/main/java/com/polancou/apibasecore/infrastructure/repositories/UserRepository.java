package com.polancou.apibasecore.infrastructure.repositories;

import com.polancou.apibasecore.domain.models.Usuario;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.JpaSpecificationExecutor;
import org.springframework.stereotype.Repository;

import java.util.Optional;
import java.util.UUID;

@Repository
public interface UserRepository extends JpaRepository<Usuario, UUID>, JpaSpecificationExecutor<Usuario> {
    
    Optional<Usuario> findByEmail(String email);
    
    boolean existsByEmail(String email);
    
    boolean existsByTaxId(String taxId);
    
    Optional<Usuario> findByTaxId(String taxId);
    
    Optional<Usuario> findByEmailVerificationToken(String token);
    
    Optional<Usuario> findByPasswordResetToken(String token);
}
