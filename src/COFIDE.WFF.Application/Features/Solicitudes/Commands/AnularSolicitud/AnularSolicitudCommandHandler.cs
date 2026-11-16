using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AnularSolicitud;

public sealed class AnularSolicitudCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudRepository solicitudRepository,
        IAccesoEspecialRepository accesoEspecialRepository
    ) : BaseCommandHandler<AnularSolicitudCommand, string>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public override async Task<string> Handle(
    AnularSolicitudCommand request,
    CancellationToken cancellationToken)
    {

        var solicitud = await _solicitudRepository.ObtenerAgregadoSolicitudAsync(
            request.IdSolicitud,
            cancellationToken,
            tracking: true);

        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        long idUsuario = _currentUser.UserIdLocal!.Value;
        var rolUsuario = _currentUser.RolUsuario
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);
        var usuario = new Usuario(idUsuario, rolUsuario);
        var areasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);

        solicitud.Anular(usuario, _currentUser.UserAreaId, areasAccesoEspecial);
        return solicitud.CodigoSolicitud;

    }
}

