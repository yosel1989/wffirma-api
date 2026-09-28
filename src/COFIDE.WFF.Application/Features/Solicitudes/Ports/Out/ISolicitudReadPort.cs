using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudReadPort
{
    Task<SolicitudDocumento?> ObtenerDocumentoParaFirmaInternaMasivaRead(long idSolicitud, long idSolicitudDocumento, CancellationToken cancellationToken, bool tracking = false);
    Task<List<SolicitudDocumento>> ObtenerDocumentosParaFirmaInternaMasivaRead(long idSolicitud, List<long> documentos, CancellationToken cancellationToken, bool tracking = false);
}
