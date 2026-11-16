using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public sealed class SolicitudConsultaAuthorizationService(
    ICurrentUserService currentUser,
    ISolicitudQueryRepository solicitudQueryRepository,
    IAccesoEspecialRepository accesoEspecialRepository
) : ISolicitudConsultaAuthorizationService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public async Task AutorizarPorIdAsync(long idSolicitud, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is not long idUsuario)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var solicitud = await _solicitudQueryRepository.ObtenerParaAutorizacionAsync(
            idSolicitud, cancellationToken);
        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        var areasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);

        ReglaConsultaSolicitud.Validar(
            solicitud,
            idUsuario,
            _currentUser.UserAreaId,
            _currentUser.CodigoRolUsuario,
            areasAccesoEspecial);
    }

    public async Task AutorizarReusoPorIdAsync(long idSolicitud, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is not long idUsuario || _currentUser.RolUsuario is not { } rolUsuario)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var solicitud = await _solicitudQueryRepository.ObtenerParaAutorizacionAsync(
            idSolicitud, cancellationToken);
        if (solicitud is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudOrigenNotFound);

        var areasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);
        solicitud.ValidarPermisoReuso(
            new Usuario(idUsuario, rolUsuario),
            _currentUser.UserAreaId,
            areasAccesoEspecial);
    }

    public async Task AutorizarSolicitudAsync(Solicitud? solicitud)
    {
        if (_currentUser.UserIdLocal is not long idUsuario)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var areasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);

        ReglaConsultaSolicitud.Validar(
            solicitud,
            idUsuario,
            _currentUser.UserAreaId,
            _currentUser.CodigoRolUsuario,
            areasAccesoEspecial);
    }
}
