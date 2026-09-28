using COFIDE.WFF.Application.Features.Solicitudes.Validators;
using FluentValidation;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;

public class RegistrarSolicitudValidator : AbstractValidator<RegistrarSolicitudCommand>
{
    public RegistrarSolicitudValidator()
    {
        this.AgregarReglasBasicas(
             x => x.IdTipoSolicitud,
             x => x.IdSubTipoSolicitud,
             x => x.IdtipoOperacion,
             x => x.Asunto);
    }
}
