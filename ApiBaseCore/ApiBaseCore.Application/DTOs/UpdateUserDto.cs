using System.ComponentModel.DataAnnotations;

namespace ApiBaseCore.Application.DTOs;

public class UpdateUserDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? TaxId { get; set; }

    public string? Password { get; set; }
}
