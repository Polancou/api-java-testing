using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartDtoValidator()
    {
        RuleFor(expression: x => x.ProductId)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "El ID del producto debe ser válido.");

        RuleFor(expression: x => x.Quantity)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "La cantidad debe ser mayor a 0.");
    }
}
