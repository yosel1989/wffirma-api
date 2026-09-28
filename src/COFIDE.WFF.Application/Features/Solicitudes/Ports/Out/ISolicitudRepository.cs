using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Pageds;
using COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudRepository : IGenericRepository<Solicitud, long>
{
    Task<long> ObtenerSiguienteIdSolFirmanteAsync(CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdSolFirmanteAsync(long cantidad, CancellationToken cancellationToken);
    Task<int> ContarListadoSolicitud(CancellationToken cancellationToken);
    Task<Solicitud?> ObtenerParaAprobacion(long id, CancellationToken cancellationToken);
    Task<FormatoNotificacionSolicitud?> ObtenerParaNotificaciones(long id, CancellationToken cancellationToken);
    Task<Solicitud?> ObtenerSolicitudParaRevision(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerSolicitudParaAprobacion(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerSolicitudParaRechazo(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerAgregadoSolicitudAsync(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<Solicitud?> ObtenerSolicitudConDocumentosYSustentosPorIdAsync(long id, List<long>? idsDocumentos, List<long>? idsSustentos, CancellationToken cancellationToken, bool tracking = false);
    void GuardarFlujoSolicitudAsync(Solicitud solicitud);
    Task<IReadOnlyCollection<CodigoReferenciaExistente>> ObtenerCodigosExistentesAsync(long? idSolicitudExcluir, IReadOnlyCollection<string> codigos, CancellationToken cancellationToken);

    #region PAGINADO

    Task<PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>> ObtenerPaginado(
        PagedQueryParams paginationParams,
        long? codigoUsuario,
        long? areaUsuario,
        string? roLUsuario,
        long? departamentoUsuario,
        string? fechainicio,
        string? fechafin,
        List<long>? gerencia,
        List<long>? departamento,
        List<long>? solicitante,
        List<long>? tiposolicitud,
        List<long>? subtiposolicitud,
        List<long>? tipoOperacion,
        List<long>? aprobadorActual,
        List<long>? estado,
        string? codigosolicitud,
        string? asunto,
        string? referencia,
        bool? urgente,
        bool? usuario,
        bool? usuariodepartamento,
        bool? incluirresumen,
        CancellationToken cancellationToken);


    Task<PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>> ObtenerBandejaEntrada(
      PagedQueryParams paginationParams,
      long? codigoUsuario,
      string? fechainicio,
      string? fechafin,
      List<long>? gerencia,
      List<long>? departamento,
      List<long>? solicitante,
      List<long>? tiposolicitud,
      List<long>? subtiposolicitud,
      List<long>? tipooperacion,
      string? codigosolicitud,
      string? asunto,
      string? referencia,
      bool? urgente,
      bool? incluirresumen,
      CancellationToken cancellationToken);

    #endregion

    #region SOLICITUD SUSTENTO

    Task<SolicitudSustento?> ObtenerSolicitudSustentoPorId(long id, CancellationToken cancellationToken, bool tracking = false);

    #endregion

    #region SOLICITUD DOCUMENTO 

    Task<SolicitudDocumento?> ObtenerSolicitudDocumentoPorId(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<IReadOnlyCollection<ArchivoDescargaDto>> ObtenerDocumentosFirmadosPorSolicitudAsync(
       long idSolicitud,
       CancellationToken cancellationToken,
       bool tracking = false);

    #endregion
}