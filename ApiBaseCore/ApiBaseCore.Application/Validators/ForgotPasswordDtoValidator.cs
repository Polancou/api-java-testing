using ApiBaseCore.Application.DTOs;
using FluentValidation;

namespace ApiBaseCore.Application.Validators;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(expression: x => x.Email)
            .NotEmpty().WithMessage(errorMessage: "El email es obligatorio.")
            .EmailAddress().WithMessage(errorMessage: "El formato del email no es válido.");
    }
}