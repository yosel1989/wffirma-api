using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Commands.CambiarEstadoPlantilla;

public sealed class CambiarEstadoPlantillaCommandHandler(
    IMapper mapper,
    ICurrentUserService currentUser,
    IPlantillaRepository plantillaRepository
) : BaseCommandHandler<CambiarEstadoPlantillaCommand, bool>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;

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

        var usuario = new Usuario(usuarioActual, rolUsuario);
        plantilla.PermitirActualizarPlantilla(
            usuarioActual,
            _currentUser.UserGerenciaNombre!,
            usuario);

        if (request.Activo)
            plantilla.Activar();
        else
            plantilla.Desactivar();

        await _plantillaRepository.UpdateAsync(plantilla);
        return plantilla.Estado;
    }
}
