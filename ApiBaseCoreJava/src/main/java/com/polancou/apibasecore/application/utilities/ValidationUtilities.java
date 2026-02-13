package com.polancou.apibasecore.application.utilities;

import java.util.regex.Pattern;

public class ValidationUtilities {

    // Regex for Mexican RFC
    private static final Pattern RFC_PATTERN = Pattern.compile("^[A-Z&Ñ]{3,4}\\d{6}[A-Z0-9]{3}$", Pattern.CASE_INSENSITIVE);

    public static boolean validateTaxIdRFC(String taxId) {
        if (taxId == null || taxId.isBlank()) {
            return false;
        }
        return RFC_PATTERN.matcher(taxId).matches();
    }

    public static boolean validatePhoneAndresFormat(String phone) {
        if (phone == null || phone.isBlank()) {
            return false;
        }
        // Remove non-digit characters
        String cleanPhone = phone.replaceAll("[^\\d]", "");
        // Must be exactly 10 digits
        return cleanPhone.length() == 10;
    }
}
