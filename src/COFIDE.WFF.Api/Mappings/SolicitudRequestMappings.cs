using COFIDE.WFF.Api.Request.Solicitudes;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DetectarConflictosCodigosReferencia;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;

namespace COFIDE.WFF.Api.Mappings
{
    public static class SolicitudRequestMappings
    {
        public static RegistrarSolicitudCommand ToRegistrarCommand(this CrearSolicitudRequest r) =>
            new(
                r.IdTipoSolicitud,
                r.IdSubTipoSolicitud,
                r.IdtipoOperacion,
                r.Asunto,
                r.Mensaje,
                r.EsUrgente,
                r.InCodigo,
                r.FechaLimite,
                r.PesoTotalDocu,
                r.Documentos.ToArchivosEntrada(),
                r.Sustentos.ToArchivosEntrada(),
                r.CodigosReferencia,
                r.IdPlantilla,
                r.Configuracion,
                r.Correos,
                r.Tags);

        public static EnviarSolicitudCommand ToEnviarCommand(this CrearSolicitudRequest r) =>
            new(
                r.IdTipoSolicitud,
                r.IdSubTipoSolicitud,
                r.IdtipoOperacion,
                r.Asunto,
                r.Mensaje,
                r.EsUrgente,
                r.InCodigo,
                r.FechaLimite,
                r.PesoTotalDocu,
                r.Documentos.ToArchivosEntrada(),
                r.Sustentos.ToArchivosEntrada(),
                r.CodigosReferencia,
                r.IdPlantilla,
                r.Configuracion,
                r.Correos,
                r.Tags);

        public static SolicitudRequestDto ToSolicitudRequestDto(this SolicitudFormRequest r) =>
            new(
                IdTipoSolicitud: r.IdTipoSolicitud,
                IdSubTipoSolicitud: r.IdSubTipoSolicitud,
                IdtipoOperacion: r.IdtipoOperacion,
                Asunto: r.Asunto,
                Mensaje: r.Mensaje,
                EsUrgente: r.EsUrgente,
                InCodigo: r.InCodigo,
                FechaLimite: r.FechaLimite,
                PesoTotalDocu: r.PesoTotalDocu,
                Documentos: r.Documentos.ToArchivosEntrada(),
                Sustentos: r.Sustentos.ToArchivosEntrada(),
                IdsDocumentosConservar: r.IdsDocumentosConservar,
                IdsSustentosConservar: r.IdsSustentosConservar,
                CodigosReferencia: r.CodigosReferencia,
                IdPlantilla: r.IdPlantilla,
                Configuracion: r.Configuracion,
                Correos: r.Correos,
                Tags: r.Tags);

        public static AprobarSolicitudCommand ToCommand(this AprobarSolicitudRequest r, long id) =>
            new(id, r.Comentario, r.Sustentos.ToArchivosEntrada());

        public static RechazarSolicitudCommand ToCommand(this RechazarSolicitudRequest r, long id) =>
            new(id, r.MotivoRechazo, r.Comentario, r.Sustentos.ToArchivosEntrada());

        public static ValidarDocumentoFirmadoQuery ToQuery(this ValidarDocumentoFirmadoRequest r) =>
            new(r.Archivos.Select(a => a.ToArchivoEntrada()).ToList());

        public static ObtenerSolicitudPaginadoQuery ToQuery(this SolicitudPaginadoRequest r) =>
            new(r.Fechainicio, r.Fechafin, r.Gerencia, r.Departamento, r.Solicitante, r.Tiposolicitud,
                r.Subtiposolicitud, r.TipoOperacion, r.AprobadorActual, r.Estado, r.Codigosolicitud, r.Asunto,
                r.Referencia, r.Urgente, r.Usuario, r.Usuariodepartamento, r.IncluirResumen)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };

        public static SeguimientoSolicitudesQuery ToQuery(this SeguimientoSolicitudesRequest r) =>
            new(r.Fechainicio, r.Fechafin, r.Gerencia, r.Departamento, r.Solicitante, r.Tiposolicitud, r.Subtiposolicitud,
                r.TipoOperacion, r.Asunto, r.Referencia, r.Estado, r.Firmante, r.Codigosolicitud, r.Encurso, r.Aprobada, r.Rechazada, r.IncluirResumen)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };

        public static ObtenerBandejaEntradaQuery ToQuery(this BandejaEntradaRequest r) =>
            new(r.Fechainicio, r.Fechafin, r.Gerencia, r.Departamento, r.Solicitante, r.Tiposolicitud,
                r.Subtiposolicitud, r.TipoOperacion, r.Codigosolicitud, r.Asunto, r.Referencia, r.Urgente,
                r.IncluirResumen)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };

        public static ObtenerDocumentoBloqueadoPaginadoQuery ToQuery(this DocumentoBloqueadoPaginadoRequest r) =>
            new(r.FechaInicio, r.FechaFin, r.NombreArchivo, r.CodigoSolicitud, r.FirmanteActual)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };

        public static DescargarReporteSolicitudExcelQuery ToQuery(this ReporteSolicitudRequest r) =>
            new(r.Fechainicio, r.Fechafin, r.Gerencia, r.Departamento, r.Solicitante, r.Tiposolicitud,
                r.Subtiposolicitud, r.TipoOperacion, r.AprobadorActual, r.Estado, r.Codigosolicitud, r.Asunto,
                r.Referencia, r.Urgente, r.Usuario, r.Usuariodepartamento);

        public static DetectarCodigoQuery ToQuery(this DetectarCodigosReferenciaRequest r) =>
            new(r.IdSolicitud, r.CodigosReferencia);
    }
}
