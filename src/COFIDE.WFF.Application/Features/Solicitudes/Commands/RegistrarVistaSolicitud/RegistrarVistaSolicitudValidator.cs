using COFIDE.WFF.Application.Common.Errors;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarVistaSolicitud;

public sealed class RegistrarVistaSolicitudValidator : AbstractValidator<RegistrarVistaSolicitudCommand>
{
    public RegistrarVistaSolicitudValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Solicitud debe ser un identificador positivo.")
            .WithErrorCode(ApplicationErrors.SolicitudInvalid.Code);
    }
}
