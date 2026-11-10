using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;
using COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface ISolicitudQueryRepository
{

    Task<List<SolicitudPaginadoDto>> ObtenerListadoSolicitud(ReporteSolicitudExcelRequest request, CancellationToken cancellationToken, bool tracking = false);


    Task<SolicitudPanelControlDto?> ObtenerSolicitudPanelControl(long? codigoUsuario, CancellationToken cancellationToken, bool tracking = false);

    Task<SolicitudDto?> ObtenerSolicitudPorId(long id, long? idUsuario, bool esReuso, CancellationToken cancellationToken, bool tracking = false);

    #region NOTIFICACIONES Y CONSULTAS PUNTUALES

    Task<FormatoNotificacionSolicitud?> ObtenerParaNotificaciones(long id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CodigoReferenciaExistente>> ObtenerCodigosExistentesAsync(long? idSolicitudExcluir, IReadOnlyCollection<string> codigos, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ArchivoDescargaDto>> ObtenerDocumentosFirmadosPorSolicitudAsync(
       long idSolicitud,
       CancellationToken cancellationToken,
       bool tracking = false);

    #endregion

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
        List<long>? idsAreasAccesoEspecial,
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
}

