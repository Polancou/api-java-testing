package com.polancou.apibasecore.infrastructure.repositories;

import com.polancou.apibasecore.domain.models.Address;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface AddressRepository extends JpaRepository<Address, Integer> {
}
