using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.In;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;

public sealed class EditarSolicitudCommandHandler(
        IMapper mapper,
        IEditarSolicitudUseCase usecase,
        IPlantillaRepository plantillaRepository,
        ISolicitudRepository solicitudRepository
    ) : BaseCommandHandler<EditarSolicitudCommand, string?>(mapper)
{
    private readonly IEditarSolicitudUseCase _usecase = usecase;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;

    public override async Task<string?> Handle(EditarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        var solicitudPlantilla = await _usecase.Editar(cmd.Request, cmd.IdSolicitud, true, cancellationToken);
        Solicitud solicitud = solicitudPlantilla.solicitud;
        Plantilla? plantilla = solicitudPlantilla.plantilla;

        _plantillaRepository.GuardarFlujoPlantillaAsync(plantilla);
        _solicitudRepository.GuardarFlujoSolicitudAsync(solicitud);
        return solicitud.CodigoSolicitud;
    }
}