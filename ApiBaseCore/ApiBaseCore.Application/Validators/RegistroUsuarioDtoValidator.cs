using ApiBaseCore.Application.DTOs;
using FluentValidation;

namespace ApiBaseCore.Application.Validators;

/// <summary>
/// Define las reglas de validación para el DTO de registro de usuario (RegistroUsuarioDto).
/// Hereda de AbstractValidator, que es la clase base de FluentValidation.
/// </summary>
public class RegistroUsuarioDtoValidator : AbstractValidator<RegistroUsuarioDto>
{
    public RegistroUsuarioDtoValidator()
    {
        // Regla para NombreCompleto
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El número de teléfono es obligatorio.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("El número de teléfono no es válido.");
    }
}