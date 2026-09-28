using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;
namespace COFIDE.WFF.Application.Features.Revisiones.Models;

public sealed record EditarFlujoRequest
(
   List<SolicitudEtapaRequest>? Etapas
);

