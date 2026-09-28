using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.PermisoEditarPlantilla;

public sealed class PermisoEditarPlantillaQueryHander(
        IMapper mapper,
        ICurrentUserService currentUser,
        IPlantillaRepository plantillaRepository
    ) : BaseQueryHandler<PermisoEditarPlantillaQuery, bool>(mapper)
{    
    private readonly ICurrentUserService _currentUser= currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;

    public override async Task<bool> Handle(PermisoEditarPlantillaQuery request, CancellationToken cancellationToken)
    {
        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;
        var rolUusario = _currentUser.RolUsuario;
        var usuario = new Usuario(usuarioActual, rolUusario!.Value);

        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(request.Id, cancellationToken, true);
        if (plantilla is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        return plantilla.ValidarPermisoPlantilla(usuarioActual, gerenciaUsuario, usuario);
    }
}
