using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

public sealed class ObtenerPlantillaPorIdQueryHandler(
        IMapper mapper,
        IPlantillaRepository plantillaRepository
    ) : BaseQueryHandler<ObtenerPlantillaPorIdQuery, PlantillaDto?>(mapper)
{
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;

    public override async Task<PlantillaDto?> Handle(ObtenerPlantillaPorIdQuery request, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillaRepository.ObtenerDetallePlantillaPorId(request.IdPlantilla) ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);
        return plantilla;
    }
}