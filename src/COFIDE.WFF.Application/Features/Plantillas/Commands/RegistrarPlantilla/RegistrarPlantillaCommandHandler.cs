using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using ApplicationException = COFIDE.WFF.Application.Exceptions.ApplicationException;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;

public sealed class RegistrarPlantillaCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IPlantillaRepository plantillaRepository,
        IPlantillaCreacionService plantillaCreacionService
    ) : BaseCommandHandler<RegistrarPlantillaCommand, long>(mapper)
{
    private readonly ICurrentUserService _currentUser= currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IPlantillaCreacionService _plantillaCreacionService= plantillaCreacionService;

    public override async Task<long> Handle(RegistrarPlantillaCommand request, CancellationToken cancellationToken)
    {
        long? idAreaPlantilla = _currentUser.UserAreaId;
        long? idAreaPadrePlantilla = _currentUser.UserAreaPadreId;
        if (!idAreaPlantilla.HasValue || !idAreaPadrePlantilla.HasValue)
            throw new ApplicationException(ApplicationErrors.AreaUsuarioIncompleta);

        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;

        var input = _mapper.Map<CrearPlantillaInput>(request);
        var plantilla = await _plantillaCreacionService.CrearPlantilla(
            input,
            usuarioActual,
            gerenciaUsuario,
            idAreaPlantilla.Value,
            idAreaPadrePlantilla.Value,
            cancellationToken);
        await _plantillaRepository.AddAsync(plantilla);
        return plantilla.Id;
    }

}