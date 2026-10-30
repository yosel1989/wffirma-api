using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.ValueObjects;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.CambiarEstadoSolicitud;

public class CambiarEstadoSolicitudValidator : AbstractValidator<CambiarEstadoSolicitudCommand>
{
    private static readonly long[] EstadosDestino =
    [
        EstadoSolicitud.Borrador.Codigo,
        EstadoSolicitud.EnCurso.Codigo,
        EstadoSolicitud.Rechazado.Codigo
    ];

    public CambiarEstadoSolicitudValidator()
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0).WithMessage("Solicitud es obligatorio y debe ser distinto de cero.").WithErrorCode("SOLICITUD_INVALID");

        RuleFor(x => x.IdNuevoEstado)
            .Cascade(CascadeMode.Stop)
            .Must(id => EstadosDestino.Contains(id)).WithMessage("El nuevo estado es inválido.").WithErrorCode("NUEVO_ESTADO_INVALID");

        RuleFor(x => x.Motivo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El motivo del cambio es obligatorio.").WithErrorCode("MOTIVO_CAMBIO_REQUIRED")
            .Must(m => m.Trim().Length <= Solicitud.LongitudMaximaMotivoCambioEstado)
            .WithMessage($"El motivo del cambio no debe exceder los {Solicitud.LongitudMaximaMotivoCambioEstado} caracteres.")
            .WithErrorCode("MOTIVO_CAMBIO_LENGTH");
    }
}
