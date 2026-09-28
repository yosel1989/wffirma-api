using COFIDE.WFF.Application.Features.Solicitudes.Validators;
using FluentValidation;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;

public class EnviarSolicitudValidator : AbstractValidator<EnviarSolicitudCommand>
{
    public EnviarSolicitudValidator()
    {
        this.AgregarReglasBasicas(
            x => x.IdTipoSolicitud,
            x => x.IdSubTipoSolicitud,
            x => x.IdtipoOperacion,
            x => x.Asunto);

        RuleFor(x => x)
            .Must(x => x.IdPlantilla.HasValue || !string.IsNullOrEmpty(x.Configuracion))
            .WithMessage("Debe enviar IdPlantilla o Configuracion.")
            .WithErrorCode("CONFIGURACION_REQUERIDA");
    }
}