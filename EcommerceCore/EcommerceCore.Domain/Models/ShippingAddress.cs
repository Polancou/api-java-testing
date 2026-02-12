using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

public class ShippingAddress
{
    /// <summary>
    /// Identificador único de la dirección de envío.
    /// </summary>
    [Key]
    public int Id { get; private set; }

    /// <summary>
    /// Identificador del usuario propietario de la dirección.
    /// </summary>
    [Required]
    public int UserId { get; private set; }

    /// <summary>
    /// Navegación al usuario propietario.
    /// </summary>
    [ForeignKey("UserId")]
    public Usuario User { get; private set; } = null!;

    /// <summary>
    /// Nombre descriptivo de la dirección (ej. "Casa", "Trabajo").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; private set; }

    /// <summary>
    /// Primera línea de la dirección.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AddressLine1 { get; private set; }

    /// <summary>
    /// Segunda línea de la dirección (opcional).
    /// </summary>
    [MaxLength(100)]
    public string? AddressLine2 { get; private set; }

    /// <summary>
    /// Ciudad.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string City { get; private set; }

    /// <summary>
    /// Estado o Provincia.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string State { get; private set; }

    /// <summary>
    /// Código Postal.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PostalCode { get; private set; }

    /// <summary>
    /// País.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Country { get; private set; }

    // Constructor privado para EF Core
    private ShippingAddress()
    {
    }

    /// <summary>
    /// Constructor para crear una nueva dirección de envío.
    /// </summary>
    public ShippingAddress(int userId, string name, string addressLine1, string city, string state, string postalCode,
        string country, string? addressLine2 = null)
    {
        if (userId <= 0) throw new ArgumentException("El ID de usuario no es válido.",
            nameof(userId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name),
            "El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new ArgumentNullException(nameof(addressLine1),
                "La dirección es obligatoria.");
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentNullException(nameof(city),
            "La ciudad es obligatoria.");
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentNullException(nameof(state),
                "El estado es obligatorio.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentNullException(nameof(postalCode),
                "El código postal es obligatorio.");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentNullException(nameof(country),
                "El país es obligatorio.");

        UserId = userId;
        Name = name;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// Actualiza los detalles de la dirección de envío.
    /// </summary>
    public void Update(string name, string addressLine1, string city, string state, string postalCode, string country,
        string? addressLine2 = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name),
            "El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new ArgumentNullException(nameof(addressLine1),
                "La dirección es obligatoria.");
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentNullException(nameof(city),
            "La ciudad es obligatoria.");
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentNullException(nameof(state),
                "El estado es obligatorio.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentNullException(nameof(postalCode),
                "El código postal es obligatorio.");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentNullException(nameof(country),
                "El país es obligatorio.");

        Name = name;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
