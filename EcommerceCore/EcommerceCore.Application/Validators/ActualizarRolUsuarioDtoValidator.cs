using EcommerceCore.Application.DTOs;
using FluentValidation;

namespace EcommerceCore.Application.Validators;

public class ActualizarRolUsuarioDtoValidator : AbstractValidator<ActualizarRolUsuarioDto>
{
    public ActualizarRolUsuarioDtoValidator()
    {
        // Esta regla comprueba que el valor (ya sea string o número)
        // es un miembro válido y definido del enum RolUsuario.
        RuleFor(expression: x => x.Rol)
            .IsInEnum()
            .WithMessage(errorMessage: "El rol proporcionado no es válido.");
    }
}