using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.In;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarSolicitud;

public sealed class ReusarSolicitudCommandHandler(
        IMapper mapper,
        IReusarSolicitudUseCase usecase,
        IPlantillaRepository plantillaRepository,
        ISolicitudRepository solicitudRepository
    ) : BaseCommandHandler<ReusarSolicitudCommand, string?>(mapper)
{
    private readonly IReusarSolicitudUseCase _usecase = usecase;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;

    public override async Task<string?> Handle(ReusarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        var request = _mapper.Map<SolicitudRequestDto>(cmd.Request);
        var solicitudPlantilla = await _usecase.Reusar(request, cmd.IdSolicitudOrigen, true, cancellationToken);
        Solicitud solicitud = solicitudPlantilla.Item1;
        Plantilla? plantilla = solicitudPlantilla.Item2;

        if (plantilla != null)
            await _plantillaRepository.AddAsync(plantilla);
        await _solicitudRepository.AddAsync(solicitud);
        return solicitud.CodigoSolicitud;
    }
}