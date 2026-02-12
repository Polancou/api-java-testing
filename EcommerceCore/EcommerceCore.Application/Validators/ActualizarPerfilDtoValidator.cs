using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

/// <summary>
/// Define las reglas de validación para el DTO de actualizar usuario (ActualizarPerfilDto).
/// </summary>
public class ActualizarPerfilDtoValidator : AbstractValidator<ActualizarPerfilDto>
{
    public ActualizarPerfilDtoValidator()
    {
        RuleFor(expression: x => x.NombreCompleto).NotEmpty().Length(min: 3,
            max: 100);
        RuleFor(expression: x => x.NumeroTelefono).NotEmpty();
    }
}