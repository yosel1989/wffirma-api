using FluentValidation;

namespace COFIDE.WFF.Application.Features.Usuarios.Commands.CambiarEstadoUsuario;

public sealed class CambiarEstadoUsuarioValidator : AbstractValidator<CambiarEstadoUsuarioCommand>
{
    public CambiarEstadoUsuarioValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0L)
            .WithMessage("El identificador de usuario debe ser mayor que cero.");
    }
}
