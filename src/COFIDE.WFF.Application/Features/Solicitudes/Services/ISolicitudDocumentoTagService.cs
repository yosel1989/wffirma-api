using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudDocumentoTagService
{
    Task CrearDocumentoTagDesdeSolicitud(Solicitud solicitud, string? documentosTag, CancellationToken cancellationToken);
    Task AntualizarDocumentoTagDesdeSolicitud(Solicitud solicitud, string? documentosTag, CancellationToken cancellationToken);
}
