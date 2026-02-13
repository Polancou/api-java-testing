package com.polancou.apibasecore.application.dtos;

import lombok.Data;

@Data
public class UserFilterDto {
    /**
     * Property to sort by. Allowed values: email, id, name, phone, tax_id, created_at.
     * Default sort direction is ascending unless logic specifies otherwise.
     */
    private String sortedBy;

    /**
     * Filter string in format: attribute+op+value
     * Examples: name+co+user, email+ew+mail.com
     * Ops: co (contains), eq (equals), sw (starts with), ew (ends with)
     */
    private String filter;
}
