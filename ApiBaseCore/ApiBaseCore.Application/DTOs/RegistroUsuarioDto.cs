namespace ApiBaseCore.Application.DTOs;

/// <summary>
/// Define los datos que un cliente debe enviar para registrarse.
/// Las reglas de validación para esta clase se definen en RegistroUsuarioDto.cs
/// </summary>
public class RegistroUsuarioDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Phone { get; set; }
    public string? TaxId { get; set; }
}