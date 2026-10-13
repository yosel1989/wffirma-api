using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;

namespace COFIDE.WFF.Api.Request.Revisiones
{
    /// <summary>Etapas del flujo editado de una solicitud.</summary>
    public sealed class ActualizarFlujoRequest
    {
        public List<SolicitudEtapaRequest>? Etapas { get; init; }
    }
}
