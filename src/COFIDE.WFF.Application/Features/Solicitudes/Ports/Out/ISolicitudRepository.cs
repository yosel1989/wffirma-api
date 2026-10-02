using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudRepository : IGenericRepository<Solicitud, long>
{
    Task<long> ObtenerSiguienteIdSolFirmanteAsync(CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdSolFirmanteAsync(long cantidad, CancellationToken cancellationToken);
    Task<Solicitud?> ObtenerParaAprobacion(long id, CancellationToken cancellationToken);
    Task<Solicitud?> ObtenerSolicitudParaRevision(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerSolicitudParaAprobacion(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerSolicitudParaRechazo(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerAgregadoSolicitudAsync(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerSolicitudConDocumentosYSustentosPorIdAsync(long id, List<long>? idsDocumentos, List<long>? idsSustentos, CancellationToken cancellationToken, bool tracking = false);
    void GuardarFlujoSolicitudAsync(Solicitud solicitud);


    #region SOLICITUD SUSTENTO

    Task<SolicitudSustento?> ObtenerSolicitudSustentoPorId(long id, CancellationToken cancellationToken, bool tracking = false);

    #endregion

    #region SOLICITUD DOCUMENTO 

    Task<SolicitudDocumento?> ObtenerSolicitudDocumentoPorId(long id, CancellationToken cancellationToken, bool tracking = false);

    #endregion
}