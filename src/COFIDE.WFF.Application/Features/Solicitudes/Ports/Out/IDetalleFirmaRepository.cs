using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IDetalleFirmaRepository : IGenericRepository<DetalleFirma, long>
{
    Task<List<DetalleFirma>> ObtenerDetalleFirmaPorCodigoFirma(string codigoFirma, CancellationToken cancellationToken);
    Task<DetalleFirma?> ValidarDocumentoFirmadoAsync(long idDetalleFirma, CancellationToken cancellationToken);
    Task<List<DetalleFirma>> ValidarDocumentoFirmadoAsync(List<long> idsDetalleFirma, CancellationToken cancellationToken);
    Task<DetalleFirma?> ObtenerDetalleBloqueadoPorDocumentoAsync(long idDocumento, CancellationToken cancellationToken);
    Task<int> ActualizarEstadosSolicitudProgramadoAsync(CancellationToken cancellationToken);
    Task<List<DetalleFirma>> ObtenerDocumentosEnEstadoFirmadoAsync(List<long> idsDetalleFirma,CancellationToken cancellationToken);
}
