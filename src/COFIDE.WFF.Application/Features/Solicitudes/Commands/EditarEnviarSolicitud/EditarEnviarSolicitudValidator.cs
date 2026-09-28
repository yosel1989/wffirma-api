using COFIDE.WFF.Application.Features.Solicitudes.Validators;
using FluentValidation;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarEnviarSolicitud;
public class EditarEnviarSolicitudValidator : AbstractValidator<EditarEnviarSolicitudCommand>
{
    public EditarEnviarSolicitudValidator()
    {
        this.AgregarReglasBasicas(
            x => x.Request.IdTipoSolicitud,
            x => x.Request.IdSubTipoSolicitud,
            x => x.Request.IdtipoOperacion,
            x => x.Request.Asunto);
    }
}