using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(expression: x => x.Token).NotEmpty().WithMessage(errorMessage: "El token de restablecimiento es obligatorio.");
        RuleFor(expression: x => x.NewPassword)
            .NotEmpty().WithMessage(errorMessage: "La nueva contraseña es obligatoria.")
            .MinimumLength(minimumLength: 8).WithMessage(errorMessage: "La nueva contraseña debe tener al menos 8 caracteres.")
            .Matches(expression: "[A-Z]").WithMessage(errorMessage: "La nueva contraseña debe contener al menos una mayúscula.")
            .Matches(expression: "[a-z]").WithMessage(errorMessage: "La nueva contraseña debe contener al menos una minúscula.")
            .Matches(expression: "[0-9]").WithMessage(errorMessage: "La nueva contraseña debe contener al menos un número.");

        RuleFor(expression: x => x.ConfirmPassword)
            .Equal(expression: x => x.NewPassword).WithMessage(errorMessage: "Las contraseñas nuevas no coinciden.");
    }
}
