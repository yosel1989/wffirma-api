using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;
namespace COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;

public sealed record EditarFlujoCommand
(
    long Id,
    long IdPlantilla,
    List<SolicitudEtapaRequest>? Etapas
) : UpdateCommand<EditarFlujoResponseDto, long>(Id);
