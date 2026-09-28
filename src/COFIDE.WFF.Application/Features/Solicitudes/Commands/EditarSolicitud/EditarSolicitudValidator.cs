using COFIDE.WFF.Application.Features.Solicitudes.Validators;
using FluentValidation;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;

public class EditarSolicitudValidator : AbstractValidator<EditarSolicitudCommand>
{
    public EditarSolicitudValidator()
    {
        this.AgregarReglasBasicas(
            x => x.Request.IdTipoSolicitud,
            x => x.Request.IdSubTipoSolicitud,
            x => x.Request.IdtipoOperacion,
            x => x.Request.Asunto);
    }
}