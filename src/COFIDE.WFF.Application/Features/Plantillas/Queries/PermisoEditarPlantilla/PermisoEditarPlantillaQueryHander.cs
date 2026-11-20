using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.PermisoEditarPlantilla;

public sealed class PermisoEditarPlantillaQueryHander(
        IMapper mapper,
        IPlantillaRepository plantillaRepository,
        IPlantillaCambioEstadoAuthorizationService authorizationService
    ) : BaseQueryHandler<PermisoEditarPlantillaQuery, bool>(mapper)
{
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IPlantillaCambioEstadoAuthorizationService _authorizationService = authorizationService;
    public override async Task<bool> Handle(PermisoEditarPlantillaQuery request, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(request.Id, cancellationToken, true);
        if (plantilla is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        return await _authorizationService.PuedeAutorizarAsync(plantilla, cancellationToken); ;
    }
}
