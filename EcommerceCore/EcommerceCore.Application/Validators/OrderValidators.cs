using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(expression: x => x.Status)
            .IsInEnum().WithMessage(errorMessage: "El estado del pedido no es válido.");
    }
}
