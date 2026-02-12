using System.Text.Json.Serialization;

namespace ApiBaseCore.Application.DTOs;

/// <summary>
/// Representa los datos del perfil de un usuario que son seguros para ser devueltos por la API.
/// </summary>
public class PerfilUsuarioDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    [JsonPropertyName("tax_id")]
    public string? TaxId { get; set; }
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public string Rol { get; set; }
    public List<AddressDto> Addresses { get; set; } = new();
}