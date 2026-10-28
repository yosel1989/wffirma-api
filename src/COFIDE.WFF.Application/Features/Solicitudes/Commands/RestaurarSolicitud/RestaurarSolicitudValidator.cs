using FluentValidation;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RestaurarSolicitud;

public class RestaurarSolicitudValidator : AbstractValidator<RestaurarSolicitudCommand>
{
    public RestaurarSolicitudValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0).WithMessage("Solicitud es obligatorio y debe ser distinto de cero.").WithErrorCode("SOLICITUD_INVALID");

        RuleFor(x => x.IdPuntoRestauracion)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0).WithMessage("El punto de restauración es obligatorio y debe ser distinto de cero.").WithErrorCode("PUNTO_RESTAURACION_INVALID");
    }
}
