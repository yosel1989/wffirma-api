using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.AnularSolicitud;

public sealed class AnularSolicitudCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudRepository solicitudRepository,
        ISolicitudAnulacionService solicitudAnulacionService
    ) : BaseCommandHandler<AnularSolicitudCommand, string>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ISolicitudAnulacionService _solicitudAnulacionService = solicitudAnulacionService;

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

        if (solicitud.IdUsuarioSolicitante != idUsuario)
            throw new ValidacionAplicacion.ApplicationException(
                new ApplicationError("", "Solo puede anular el usuario que creó la solicitud."));

        await _solicitudAnulacionService.AnularSolicitudAsync(
            solicitud,
            idUsuario,
            cancellationToken);
        return solicitud.CodigoSolicitud;

    }
}

