using ApiBaseCore.Application.DTOs;
using FluentValidation;

namespace ApiBaseCore.Application.Validators;

/// <summary>
/// Define las reglas de validación para el DTO de login externo (ExternalLoginDto).
/// </summary>
public class ExternalLoginDtoValidator : AbstractValidator<ExternalLoginDto>
{
    public ExternalLoginDtoValidator()
    {
        RuleFor(expression: x => x.Provider)
            .NotEmpty().WithMessage(errorMessage: "El proveedor es obligatorio.");

        RuleFor(expression: x => x.IdToken)
            .NotEmpty().WithMessage(errorMessage: "El token de ID es obligatorio.");
    }
}