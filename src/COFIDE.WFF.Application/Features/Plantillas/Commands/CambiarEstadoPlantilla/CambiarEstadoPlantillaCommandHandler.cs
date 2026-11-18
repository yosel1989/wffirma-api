using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Commands.CambiarEstadoPlantilla;

public sealed class CambiarEstadoPlantillaCommandHandler
    : BaseCommandHandler<CambiarEstadoPlantillaCommand, bool>
{
    private readonly IPlantillaRepository _plantillaRepository;
    private readonly IPlantillaCambioEstadoAuthorizationService _authorizationService;

    public CambiarEstadoPlantillaCommandHandler(
        IMapper mapper,
        IPlantillaRepository plantillaRepository,
        IPlantillaCambioEstadoAuthorizationService authorizationService)
        : base(mapper)
    {
        _plantillaRepository = plantillaRepository;
        _authorizationService = authorizationService;
    }

    public override async Task<bool> Handle(
        CambiarEstadoPlantillaCommand request,
        CancellationToken cancellationToken)
    {
        var plantilla = await _plantillaRepository.ObtenerParaActivacionAsync(
            request.Id,
            cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        await _authorizationService.AutorizarAsync(plantilla, cancellationToken);

        if (request.Activo)
            plantilla.Activar();
        else
            plantilla.Desactivar();

        await _plantillaRepository.UpdateAsync(plantilla);
        return plantilla.Estado;
    }
}
