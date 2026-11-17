using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Commands.CambiarEstadoPlantilla;

public sealed class CambiarEstadoPlantillaCommandHandler(
    IMapper mapper,
    ICurrentUserService currentUser,
    IPlantillaRepository plantillaRepository,
    IAccesoEspecialRepository accesoEspecialRepository
) : BaseCommandHandler<CambiarEstadoPlantillaCommand, bool>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public override async Task<bool> Handle(
        CambiarEstadoPlantillaCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is not long usuarioActual ||
            _currentUser.RolUsuario is not { } rolUsuario)
        {
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);
        }

        var plantilla = await _plantillaRepository.ObtenerParaActivacionAsync(
            request.Id,
            cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        var codigoRolUsuario = _currentUser.CodigoRolUsuario;
        var esRolAccesoEspecial = ReglaAccesoEspecial.EvaluarRolAccesoEspecial(codigoRolUsuario);
        List<long>? idAreasAccesoEspecial = null;

        if (esRolAccesoEspecial)
        {
            idAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(usuarioActual);
        }

        if (esRolAccesoEspecial)
        {
            var tieneAccesoAreaPropia =
                plantilla.IdAreaPlantilla == _currentUser.UserAreaId &&
                plantilla.IdAreaPadrePlantilla == _currentUser.UserAreaPadreId;
            var tieneAccesoAreaEspecial =
                (plantilla.IdAreaPlantilla.HasValue &&
                 idAreasAccesoEspecial?.Contains(plantilla.IdAreaPlantilla.Value) == true) ||
                (plantilla.IdAreaPadrePlantilla.HasValue &&
                 idAreasAccesoEspecial?.Contains(plantilla.IdAreaPadrePlantilla.Value) == true);

            if (!tieneAccesoAreaPropia && !tieneAccesoAreaEspecial)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.Forbidden);
        }
        else
        {
            var usuario = new Usuario(usuarioActual, rolUsuario);
            plantilla.PermitirActualizarPlantilla(
                usuarioActual,
                _currentUser.UserAreaId,
                _currentUser.UserAreaPadreId,
                usuario);
        }

        if (request.Activo)
            plantilla.Activar();
        else
            plantilla.Desactivar();

        await _plantillaRepository.UpdateAsync(plantilla);
        return plantilla.Estado;
    }
}
