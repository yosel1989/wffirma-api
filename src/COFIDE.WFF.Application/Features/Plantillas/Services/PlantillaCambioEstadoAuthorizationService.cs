using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Common.Enums;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Services;

/// <summary>
/// Aplica las reglas vigentes de autorización para activar, desactivar o eliminar plantillas.
/// </summary>
public sealed class PlantillaCambioEstadoAuthorizationService(
    ICurrentUserService currentUser,
    IAccesoEspecialRepository accesoEspecialRepository
) : IPlantillaCambioEstadoAuthorizationService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository =
        accesoEspecialRepository;

    /// <inheritdoc />
    public async Task AutorizarAsync(
        Plantilla plantilla,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is not long usuarioActual ||
            _currentUser.RolUsuario is not { } rolUsuario)
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.UserNotAuthenticated);
        }

        var areasAccesoEspecial = rolUsuario == RolUsuario.Coordinador
            ? await _accesoEspecialRepository.GetAreasByUserIdAsync(usuarioActual)
            : [];
        var usuario = new Usuario(usuarioActual, rolUsuario);

        if (!plantilla.PermitirCambiarEstadoPlantilla(
                usuarioActual,
                _currentUser.UserAreaId,
                _currentUser.UserAreaPadreId,
                usuario,
                areasAccesoEspecial ?? []))
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.Forbidden);
        }
    }
}
