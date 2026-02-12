using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressDtoValidator()
    {
        RuleFor(expression: x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "El nombre es obligatorio.")
            .MaximumLength(maximumLength: 50).WithMessage(errorMessage: "El nombre no puede exceder los 50 caracteres.");

        RuleFor(expression: x => x.AddressLine1)
            .NotEmpty().WithMessage(errorMessage: "La dirección es obligatoria.")
            .MaximumLength(maximumLength: 100).WithMessage(errorMessage: "La dirección no puede exceder los 100 caracteres.");

        RuleFor(expression: x => x.AddressLine2)
            .MaximumLength(maximumLength: 100).WithMessage(errorMessage: "La segunda línea de dirección no puede exceder los 100 caracteres.");

        RuleFor(expression: x => x.City)
            .NotEmpty().WithMessage(errorMessage: "La ciudad es obligatoria.")
            .MaximumLength(maximumLength: 50).WithMessage(errorMessage: "La ciudad no puede exceder los 50 caracteres.");

        RuleFor(expression: x => x.State)
            .NotEmpty().WithMessage(errorMessage: "El estado/provincia es obligatorio.")
            .MaximumLength(maximumLength: 50).WithMessage(errorMessage: "El estado/provincia no puede exceder los 50 caracteres.");

        RuleFor(expression: x => x.PostalCode)
            .NotEmpty().WithMessage(errorMessage: "El código postal es obligatorio.")
            .MaximumLength(maximumLength: 20).WithMessage(errorMessage: "El código postal no puede exceder los 20 caracteres.");

        RuleFor(expression: x => x.Country)
            .NotEmpty().WithMessage(errorMessage: "El país es obligatorio.")
            .MaximumLength(maximumLength: 50).WithMessage(errorMessage: "El país no puede exceder los 50 caracteres.");
    }
}
