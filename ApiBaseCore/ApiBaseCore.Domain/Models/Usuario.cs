using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiBaseCore.Domain.Models;

/// <summary>
/// Define los roles que un usuario puede tener en el sistema.
/// </summary>
public enum RolUsuario
{
    User,
    Admin
}

/// <summary>
/// Representa la entidad principal de un usuario en la aplicación.
/// </summary>
public class Usuario
{
    #region Propiedades

    [Key]
    [JsonPropertyName("id")]
    public Guid Id { get; private set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    [JsonPropertyName("email")]
    public string Email { get; private set; }

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; private set; }

    [Required]
    [MaxLength(20)]
    [JsonPropertyName("phone")]
    public string Phone { get; private set; }

    [JsonIgnore]
    public string? PasswordHash { get; private set; }

    [MaxLength(20)]
    [JsonPropertyName("tax_id")]
    public string? TaxId { get; private set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; private set; }

    [Required]
    [JsonIgnore]
    public RolUsuario Rol { get; private set; }

    [JsonPropertyName("addresses")]
    public List<Address> Addresses { get; private set; } = new();

    // --- Technical/Auth Fields ---

    [JsonIgnore]
    public string? AvatarUrl { get; private set; }

    [JsonIgnore]
    public string? RefreshToken { get; private set; }

    [JsonIgnore]
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    [Timestamp]
    [JsonIgnore]
    public byte[] RowVersion { get; private set; }

    [JsonIgnore]
    public string? EmailVerificationToken { get; private set; }

    [JsonIgnore]
    public bool IsEmailVerified { get; private set; }

    [JsonIgnore]
    public string? PasswordResetToken { get; private set; }

    [JsonIgnore]
    public DateTime? PasswordResetTokenExpiryTime { get; private set; }

    #endregion

    #region Constructores

    private Usuario()
    {
    }

    public Usuario(string name, string email, string phone, RolUsuario rol, string? taxId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name), "El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "El email es obligatorio.");

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        Rol = rol;
        TaxId = taxId;
        CreatedAt = DateTime.UtcNow;
        IsEmailVerified = false;
    }

    #endregion

    #region Métodos de Modificación

    public void EstablecerPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentNullException(nameof(passwordHash), "El hash de la contraseña no puede estar vacío.");
        PasswordHash = passwordHash;
    }

    public void UpdateProfile(string name, string phone, string? taxId)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (!string.IsNullOrWhiteSpace(phone)) Phone = phone;
        if (taxId != null) TaxId = taxId;
    }

    public void AddAddress(Address address)
    {
        Addresses.Add(address);
    }

    public void RemoveAddress(int addressId)
    {
        var address = Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address != null)
        {
            Addresses.Remove(address);
        }
    }

    public void SetAvatarUrl(string nuevoUrl)
    {
        if (!string.IsNullOrWhiteSpace(nuevoUrl)) AvatarUrl = nuevoUrl;
    }
    
    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }
    
    public void SetRol(RolUsuario rol)
    {
        Rol = rol;
    }

    public void SetEmailVerificationToken(string token)
    {
        EmailVerificationToken = token;
    }

    public void MarkEmailAsVerified()
    {
        IsEmailVerified = true;
        EmailVerificationToken = null;
    }

    public void SetPasswordResetToken(string token, DateTime expiryTime)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiryTime = expiryTime;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiryTime = null;
    }

    #endregion
}