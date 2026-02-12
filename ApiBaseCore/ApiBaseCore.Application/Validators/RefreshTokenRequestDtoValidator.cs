using ApiBaseCore.Application.DTOs;
using FluentValidation;

namespace ApiBaseCore.Application.Validators;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(expression: x => x.RefreshToken)
            .NotEmpty().WithMessage(errorMessage: "El token de refresco es obligatorio.");
    }
}
