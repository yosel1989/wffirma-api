using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using AppException = COFIDE.WFF.Application.Exceptions.ApplicationException;

namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarVistaSolicitud;

public sealed class RegistrarVistaSolicitudCommandHandler(
    IMapper mapper,
    ICurrentUserService currentUser,
    ISolicitudRepository solicitudRepository)
    : BaseCommandHandler<RegistrarVistaSolicitudCommand, DateTime>(mapper)
{
    public override async Task<DateTime> Handle(RegistrarVistaSolicitudCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserIdLocal is not long idUsuario)
            throw new AppException(ApplicationErrors.UserNotAuthenticated);

        var firmantes = await solicitudRepository.ObtenerFirmantesParaVisualizacionAsync(request.Id, idUsuario, cancellationToken);
        if (firmantes.Count == 0)
            throw new AppException(ApplicationErrors.SolicitudSinAcceso);

        // El UnitOfWork guarda solo la fecha modificada de las filas del usuario autenticado.
        var fecha = firmantes[0].RegistrarVistaSolicitud();
        foreach (var firmante in firmantes.Skip(1))
            firmante.RegistrarVistaSolicitud();
        return fecha;
    }
}
