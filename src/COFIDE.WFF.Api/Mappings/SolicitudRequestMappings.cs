using COFIDE.WFF.Api.Request.Solicitudes;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
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
    }

}
