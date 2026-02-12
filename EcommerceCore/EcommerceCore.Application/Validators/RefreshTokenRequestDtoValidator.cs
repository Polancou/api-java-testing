using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(expression: x => x.RefreshToken)
            .NotEmpty().WithMessage(errorMessage: "El token de refresco es obligatorio.");
    }
}
