using FluentValidation;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;

public class AprobarSolicitudValidator : AbstractValidator<AprobarSolicitudCommand>
{
    public AprobarSolicitudValidator()
    {
        RuleFor(x => x.Id)
          .Cascade(CascadeMode.Stop)
          .NotNull().WithMessage("Solicitud es obligatorio.").WithErrorCode("SOLICITUD_REQUIRED")
          .NotEqual(0).WithMessage("Solicitud es obligatorio y debe ser distinto de cero.").WithErrorCode("SOLICITUD_INVALID");
    }
}