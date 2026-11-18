using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Commands.EliminarPlantilla;

/// <summary>
/// Procesa la eliminación lógica de una plantilla existente.
/// </summary>
public sealed class EliminarPlantillaCommandHandler(
    IMapper mapper,
    IPlantillaRepository plantillaRepository,
    IPlantillaCambioEstadoAuthorizationService authorizationService
) : BaseCommandHandler<EliminarPlantillaCommand, bool>(mapper)
{
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IPlantillaCambioEstadoAuthorizationService _authorizationService =
        authorizationService;

    /// <inheritdoc />
    public override async Task<bool> Handle(
        EliminarPlantillaCommand request,
        CancellationToken cancellationToken)
    {
        var plantilla = await _plantillaRepository.ObtenerParaCambioEstadoAsync(
            request.Id,
            cancellationToken);

        if (plantilla is null)
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.PlantillaYaNoExiste);
        }

        if (plantilla.EstadoPlantilla == EstadoPlantilla.Eliminada)
        {
            throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.PlantillaYaNoExiste);
        }

        await _authorizationService.AutorizarAsync(plantilla, cancellationToken);
        plantilla.EliminarLogicamente();

        await _plantillaRepository.UpdateAsync(plantilla);

        return true;
    }
}
