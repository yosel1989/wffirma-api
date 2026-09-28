using FluentValidation;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;


public class RechazarSolicitudValidator : AbstractValidator<RechazarSolicitudCommand>
{
    public RechazarSolicitudValidator()
    {
        RuleFor(x => x.Id)
          .Cascade(CascadeMode.Stop)
          .NotNull().WithMessage("Solicitud es obligatorio.").WithErrorCode("SOLICITUD_REQUIRED")
          .NotEqual(0).WithMessage("Solicitud es obligatorio y debe ser distinto de cero.").WithErrorCode("SOLICITUD_INVALID");

        RuleFor(x => x.MotivoRechazo)
          .Cascade(CascadeMode.Stop)
          .NotNull().WithMessage("Motivo de rechazo es obligatorio.").WithErrorCode("MOTIVO_RECHAZO_REQUIRED");

    }
}