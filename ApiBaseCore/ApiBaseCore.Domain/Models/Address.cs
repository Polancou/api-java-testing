using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiBaseCore.Domain.Models;

public class Address
{
    [Key]
    public int Id { get; private set; }

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; private set; }

    [Required]
    [MaxLength(200)]
    [JsonPropertyName("street")]
    public string Street { get; private set; }

    [Required]
    [MaxLength(2)]
    [JsonPropertyName("country_code")]
    public string CountryCode { get; private set; }

    public Guid UsuarioId { get; private set; }
    
    [JsonIgnore]
    public Usuario Usuario { get; private set; }

    private Address() { }

    public Address(string name, string street, string countryCode)
    {
        Name = name;
        Street = street;
        CountryCode = countryCode;
    }
}
