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
        RuleFor(expression: x => x.NombreCompleto)
            .NotEmpty().WithMessage(errorMessage: "El nombre es obligatorio.")
            .Length(min: 3,
                max: 100).WithMessage(errorMessage: "El nombre debe tener entre 3 y 100 caracteres.");

        // Regla para Email
        RuleFor(expression: x => x.Email)
            .NotEmpty().WithMessage(errorMessage: "El email es obligatorio.")
            .EmailAddress().WithMessage(errorMessage: "El formato del email no es válido.");

        // Regla para Password
        RuleFor(expression: x => x.Password)
            .NotEmpty().WithMessage(errorMessage: "La contraseña es obligatoria.")
            .MinimumLength(minimumLength: 8).WithMessage(errorMessage: "La contraseña debe tener al menos 8 caracteres.")
            .Matches(expression: "[A-Z]").WithMessage(errorMessage: "La contraseña debe contener al menos una mayúscula.")
            .Matches(expression: "[a-z]").WithMessage(errorMessage: "La contraseña debe contener al menos una minúscula.")
            .Matches(expression: "[0-9]").WithMessage(errorMessage: "La contraseña debe contener al menos un número.");

        // Regla para NumeroTelefono
        RuleFor(expression: x => x.NumeroTelefono)
            .NotEmpty().WithMessage(errorMessage: "El número de teléfono es obligatorio.");
    }
}