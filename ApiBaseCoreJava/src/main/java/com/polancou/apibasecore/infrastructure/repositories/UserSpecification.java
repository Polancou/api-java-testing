package com.polancou.apibasecore.infrastructure.repositories;

import com.polancou.apibasecore.domain.models.Usuario;
import jakarta.persistence.criteria.CriteriaBuilder;
import jakarta.persistence.criteria.CriteriaQuery;
import jakarta.persistence.criteria.Predicate;
import jakarta.persistence.criteria.Root;
import org.springframework.data.jpa.domain.Specification;

public class UserSpecification {

    public static Specification<Usuario> filterBy(String filterString) {
        return (Root<Usuario> root, CriteriaQuery<?> query, CriteriaBuilder cb) -> {
            if (filterString == null || filterString.isBlank()) {
                return cb.conjunction();
            }

            String[] parts = filterString.split("[ +]");
            if (parts.length < 3) {
                return cb.conjunction();
            }

            String attribute = parts[0].toLowerCase().trim();
            String op = parts[1].toLowerCase().trim();
            // Join the rest as value (in case value had spaces, though split might affect it. 
            // C# logic: string.Join(" ", parts.Skip(2)). 
            // Here split("[ +]") splits by space OR plus.
            // If value has spaces, they are split. We need to rejoin.
            StringBuilder valueBuilder = new StringBuilder();
            for (int i = 2; i < parts.length; i++) {
                valueBuilder.append(parts[i]);
                if (i < parts.length - 1) valueBuilder.append(" ");
            }
            String value = valueBuilder.toString();

            switch (attribute) {
                case "name":
                    return applyOp(cb, root.get("name"), op, value);
                case "email":
                    return applyOp(cb, root.get("email"), op, value);
                case "phone":
                    return applyOp(cb, root.get("phone"), op, value);
                case "tax_id":
                    return applyOp(cb, root.get("taxId"), op, value);
                case "id":
                    if ("eq".equals(op)) {
                        try {
                            return cb.equal(root.get("id"), java.util.UUID.fromString(value));
                        } catch (IllegalArgumentException e) {
                            return cb.conjunction(); // Invalid UUID, return nothing or all? C# returned query (all) if parse failed.
                        }
                    }
                    return cb.conjunction();
                default:
                    return cb.conjunction();
            }
        };
    }

    private static Predicate applyOp(CriteriaBuilder cb, jakarta.persistence.criteria.Path<String> path, String op, String value) {
        switch (op) {
            case "co": // contains
                return cb.like(cb.lower(path), "%" + value.toLowerCase() + "%");
            case "eq": // equals
                return cb.equal(cb.lower(path), value.toLowerCase());
            case "sw": // starts with
                return cb.like(cb.lower(path), value.toLowerCase() + "%");
            case "ew": // ends with
                return cb.like(cb.lower(path), "%" + value.toLowerCase());
            default:
                return cb.conjunction();
        }
    }
}
