using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(expression: x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "El nombre del producto es obligatorio.")
            .MaximumLength(maximumLength: 200).WithMessage(errorMessage: "El nombre no puede exceder los 200 caracteres.");

        RuleFor(expression: x => x.Description)
            .NotEmpty().WithMessage(errorMessage: "La descripción es obligatoria.");

        RuleFor(expression: x => x.Price)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "El precio debe ser mayor a 0.");

        RuleFor(expression: x => x.Stock)
            .GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(errorMessage: "El stock no puede ser negativo.");
    }
}

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(expression: x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "El nombre del producto es obligatorio.")
            .MaximumLength(maximumLength: 200).WithMessage(errorMessage: "El nombre no puede exceder los 200 caracteres.");

        RuleFor(expression: x => x.Description)
            .NotEmpty().WithMessage(errorMessage: "La descripción es obligatoria.");

        RuleFor(expression: x => x.Price)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "El precio debe ser mayor a 0.");

        RuleFor(expression: x => x.Stock)
            .GreaterThanOrEqualTo(valueToCompare: 0).WithMessage(errorMessage: "El stock no puede ser negativo.");
    }
}
