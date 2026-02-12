using ApiBaseCore.Application.DTOs;
using FluentValidation;

namespace ApiBaseCore.Application.Validators;

/// <summary>
/// Define las reglas de validación para el DTO de actualizar usuario (ActualizarPerfilDto).
/// </summary>
public class ActualizarPerfilDtoValidator : AbstractValidator<ActualizarPerfilDto>
{
    public ActualizarPerfilDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.")
            .When(x => x.Name != null); // Solo validar si se envía un valor

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El número de teléfono no es válido.")
            .When(x => x.Phone != null);
        RuleFor(x => x.Phone).NotEmpty().When(x => x.Phone != null);
    }
}