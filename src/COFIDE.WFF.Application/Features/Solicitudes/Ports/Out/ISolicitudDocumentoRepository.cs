using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Application.Common.Pageds;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudDocumentoRepository : IGenericRepository<SolicitudDocumento, long>
{

    Task<PagedResult<DocumentoBloqueadoPaginadoDto>> ObtenerGestionDocumentosBloqueadosPaginado(
         PagedQueryParams paginationParams,
         string? fechaInicio,
         string? fechaFin,
         string? nombreArchivo,
         string? codigoSolicitud,
         List<long>? firmanteActual,
         CancellationToken cancellationToken);
    Task<int> ContarGestionDocumentosBloqueados(
    string? fechaInicio,
    string? fechaFin,
    string? nombreArchivo,
    string? codigoSolicitud,
    List<long>? firmanteActual,
    CancellationToken cancellationToken);

    Task<SolicitudDocumento?> ObtenerSolicitudDocumentoParaFirmadoInterno(long id, CancellationToken cancellationToken, bool tracking = false);

    #region DETALLE FIRMA
    Task<DetalleFirma?> ObtenerDetalleFirmaPorDocumentoAsignado(long idSolicitudDocumento, long idUsuarioAsignado, CancellationToken cancellationToken);
    #endregion
}
