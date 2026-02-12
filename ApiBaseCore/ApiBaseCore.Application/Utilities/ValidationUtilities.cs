using System.Text.RegularExpressions;

namespace ApiBaseCore.Application.Utilities;

public static class ValidationUtilities
{
    // Regex for Mexican RFC (Persona Física y Moral)
    // Adjust logic if "RFC format" refers to a specific other standard, but usually implies Mexican RFC in this context.
    // General pattern: [A-Z&Ñ]{3,4}[0-9]{6}[A-Z0-9]{3}
    private static readonly Regex RfcRegex = new Regex(@"^[A-Z&Ñ]{3,4}\d{6}[A-Z0-9]{3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Validates if the Tax ID has a valid RFC format.
    /// </summary>
    /// <param name="taxId">The Tax ID to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool ValidateTaxIdRFC(string taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId)) return false;
        return RfcRegex.IsMatch(taxId);
    }

    /// <summary>
    /// Validates if the phone number meets "AndresFormat".
    /// Requirement: 10 digits, could include country code.
    /// "AndresFormat" implementation details were not provided, so we assume:
    /// - It must contain at least 10 digits.
    /// - Reference implementation: "phone number should be 10 digit and could include country code"
    /// Let's assume a strict 10 digit check for the main part, but if country code is included it might be longer.
    /// FOR NOW: checks if it contains exactly 10 digits after stripping non-numeric chars, or simply is a 10 digit string.
    /// User said "phone number should be 10 digit".
    /// </summary>
    /// <param name="phone">The phone number to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool ValidatePhoneAndresFormat(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;

        // Implementation of "AndresFormat"
        // remove whitespace, dashes, parens
        var cleanPhone = Regex.Replace(phone, @"[^\d]", "");

        // Must be exactly 10 digits
        return cleanPhone.Length == 10;
    }
}
