using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using VO = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public partial class SolicitudQueryRepository(
        WorkflowFirmaDbContext context,
        IHtmlTemplateRenderer htmlTemplateRenderer
    ) : ISolicitudQueryRepository
{

    private readonly WorkflowFirmaDbContext _context = context;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;





    private static string ObtenerPrimerNombreYApellido(string? nombres, string? apellidoPaterno)
    {
        var primerNombre = string.IsNullOrWhiteSpace(nombres)
            ? string.Empty
            : nombres.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                     .FirstOrDefault() ?? string.Empty;

        apellidoPaterno ??= string.Empty;

        return $"{primerNombre} {apellidoPaterno}".Trim();
    }


    [SuppressMessage(
    "Major Code Smell",
    "S3776:Cognitive Complexity of methods should not be too high",
    Justification = "Consulta compleja de dashboard. Se mantiene temporalmente por compatibilidad con EF Core.")]
    public async Task<SolicitudPanelControlDto?> ObtenerSolicitudPanelControl(
        long? codigoUsuario,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        if (!codigoUsuario.HasValue)
            return null;

        IQueryable<DetalleFirma> detalleFirmasQuery = tracking
            ? _context.DetalleFirmas
            : _context.DetalleFirmas.AsNoTracking();

        IQueryable<SolicitudFirmanteFlujo> solicitudFirmanteFlujoQuery = tracking
            ? _context.Set<SolicitudFirmanteFlujo>()
            : _context.Set<SolicitudFirmanteFlujo>().AsNoTracking();

        IQueryable<Solicitud> solicitudesQuery = tracking
            ? _context.Set<Solicitud>()
            : _context.Set<Solicitud>().AsNoTracking();

        var estadoEnCurso = VO.EstadoSolicitud.EnCurso.Codigo;
        var estadoRechazado = VO.EstadoSolicitud.Rechazado.Codigo;

        var etapasPendientesMinimas = solicitudFirmanteFlujoQuery
            .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado &&
                !sf.RevAprobado &&
                sf.FechaRevision == null)
            .GroupBy(sf => sf.SolicitudPlantilla.Solicitud.Id)
            .Select(g => new
            {
                IdSolicitud = g.Key,
                OrdenEtapaMinima = g.Min(x => x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
            });

        var etapasPendientesMinimasSinDocumentos = solicitudFirmanteFlujoQuery
            .Where(sf =>
                sf.Estado &&
                sf.SolicitudPlantilla.Solicitud.Estado &&
                sf.SolicitudPlantilla.Solicitud.IdEstadoSolicitud == estadoEnCurso &&
                !sf.SolicitudPlantilla.Solicitud.Documentos.Any() &&
                !sf.RevAprobado)
            .GroupBy(sf => sf.SolicitudPlantilla.Solicitud.Id)
            .Select(g => new
            {
                IdSolicitud = g.Key,
                OrdenEtapaMinima = g.Min(x => x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
            });

        IQueryable<DetalleFirma> queryConDocumentos = detalleFirmasQuery
            .Where(df =>
                df.Estado &&
                codigoUsuario.HasValue &&
                df.SolicitudDocumento.Solicitud.IdEstadoSolicitud == estadoEnCurso &&
                df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Estado &&
                df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value &&

                etapasPendientesMinimas.Any(e =>
                    e.IdSolicitud == df.SolicitudDocumento.Solicitud.Id &&
                    e.OrdenEtapaMinima == df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa
                ) &&
                !df.SolicitudFirmanteFlujo.RevAprobado &&
                df.SolicitudFirmanteFlujo.FechaRevision == null &&
                (
                    df.EstadoDocumento == VO.EstadoDocumento.Pendiente ||
                    df.EstadoDocumento == VO.EstadoDocumento.Firmado ||
                    df.EstadoDocumento == VO.EstadoDocumento.Bloqueado
                ) &&

                (
                    (
                        df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == 1
                    )
                    ||
                    detalleFirmasQuery.Any(dfAnt =>
                        dfAnt.SolicitudDocumento.Solicitud.Id == df.SolicitudDocumento.Solicitud.Id &&
                        dfAnt.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id ==
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id &&
                        dfAnt.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa <
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa &&
                        dfAnt.SolicitudFirmanteFlujo.RevAprobado &&
                        dfAnt.SolicitudFirmanteFlujo.FechaRevision != null
                    )
                )
            );

        IQueryable<SolicitudFirmanteFlujo> querySinDocumentos = solicitudFirmanteFlujoQuery
            .Where(sf =>
                sf.Estado &&
                sf.SolicitudPlantilla.Solicitud.Estado &&
                sf.SolicitudPlantilla.Solicitud.IdEstadoSolicitud == estadoEnCurso &&
                sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value &&
                !sf.SolicitudPlantilla.Solicitud.Documentos.Any() &&
                !sf.RevAprobado &&
                etapasPendientesMinimasSinDocumentos.Any(e =>
                    e.IdSolicitud == sf.SolicitudPlantilla.Solicitud.Id &&
                    e.OrdenEtapaMinima == sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa
                ) &&
                (
                    (
                        sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == 1 &&
                        sf.FechaRevision == null
                    )
                    ||
                    solicitudFirmanteFlujoQuery.Any(sfAnt =>
                        sfAnt.Estado &&
                        sfAnt.SolicitudPlantilla.Solicitud.Estado &&
                        sfAnt.SolicitudPlantilla.Solicitud.Id == sf.SolicitudPlantilla.Solicitud.Id &&
                        sfAnt.FirmanteFlujo.PlantillaEtapa.Plantilla.Id == sf.FirmanteFlujo.PlantillaEtapa.Plantilla.Id &&
                        sfAnt.FirmanteFlujo.PlantillaEtapa.OrdenEtapa < sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa &&
                        sfAnt.RevAprobado &&
                        sfAnt.FechaRevision != null
                    )
                )
            );

        var pendientesConDocumentos = await queryConDocumentos
            .Select(df => new
            {
                IdSolicitud = df.SolicitudDocumento.Solicitud.Id,
                df.SolicitudDocumento.Solicitud.CodigoSolicitud,
                df.SolicitudDocumento.Solicitud.Asunto,
                df.SolicitudDocumento.Solicitud.EsUrgente,
                Solicitante = (
                    (df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres.Contains(" ")
                        ? df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres.Substring(
                            0,
                            df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres.IndexOf(" "))
                        : df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres)
                    + " " +
                    (df.SolicitudDocumento.Solicitud.UsuarioSolicitante.ApellidoPaterno ?? "")
                ).Trim(),
                AreaSolicitante = df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Area != null
                    ? df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Area.NombreArea
                    : string.Empty,
                df.SolicitudDocumento.Solicitud.FechaCreacion,
                FechaLimite = df.SolicitudDocumento.Solicitud.FechaLimite != null
                    ? df.SolicitudDocumento.Solicitud.FechaLimite.Valor
                    : (DateTime?)null,
                IdPlantilla = df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendientesSinDocumentos = await querySinDocumentos
            .Select(sf => new
            {
                IdSolicitud = sf.SolicitudPlantilla.Solicitud.Id,
                sf.SolicitudPlantilla.Solicitud.CodigoSolicitud,
                sf.SolicitudPlantilla.Solicitud.Asunto,
                sf.SolicitudPlantilla.Solicitud.EsUrgente,
                Solicitante = (
                    (sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres.Contains(" ")
                        ? sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres.Substring(
                            0,
                            sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres.IndexOf(" "))
                        : sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres)
                    + " " +
                    (sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.ApellidoPaterno ?? "")
                ).Trim(),
                AreaSolicitante = sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Area != null
                    ? sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Area.NombreArea
                    : string.Empty,
                sf.SolicitudPlantilla.Solicitud.FechaCreacion,
                FechaLimite = sf.SolicitudPlantilla.Solicitud.FechaLimite != null
                    ? sf.SolicitudPlantilla.Solicitud.FechaLimite.Valor
                    : (DateTime?)null,
                IdPlantilla = sf.FirmanteFlujo.PlantillaEtapa.Plantilla.Id
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendientesBase = pendientesConDocumentos
            .Concat(pendientesSinDocumentos)
            .GroupBy(x => new
            {
                x.IdSolicitud,
                x.CodigoSolicitud,
                x.Asunto,
                x.EsUrgente,
                x.Solicitante,
                x.AreaSolicitante,
                x.FechaCreacion,
                x.FechaLimite,
                x.IdPlantilla
            })
            .Select(g => g.First())
            .ToList();

        var totalPendientes = pendientesBase.Count;
        var totalUrgentesPendientes = pendientesBase.Count(x => x.EsUrgente);

        var pendientesTop5Base = pendientesBase
            .OrderByDescending(x => x.EsUrgente ? 1 : 0)
            .ThenBy(x => x.EsUrgente ? (x.FechaLimite ?? DateTime.MaxValue) : DateTime.MaxValue)
            .ThenBy(x => !x.EsUrgente ? x.FechaCreacion : DateTime.MaxValue)
            .Take(5)
            .ToList();

        var pendientesIds = pendientesTop5Base
            .Select(x => x.IdSolicitud)
            .Distinct()
            .ToList();

        var pendientesPlantillasIds = pendientesTop5Base
            .Select(x => x.IdPlantilla)
            .Distinct()
            .ToList();

        var firmantesPendientesRelacionados = await solicitudFirmanteFlujoQuery
            .Where(x =>
                x.Estado &&
                pendientesIds.Contains(x.SolicitudPlantilla.Solicitud.Id) &&
                pendientesPlantillasIds.Contains(x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id))
            .Select(x => new
            {
                IdSolicitud = x.SolicitudPlantilla.Solicitud.Id,
                IdPlantilla = x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id,
                x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa,
                x.RevAprobado,
                x.FechaRevision,
                Firmante = (
                    (x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                        ? x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                            0,
                            x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                        : x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                    + " " +
                    (x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                ).Trim()
            })
            .ToListAsync(cancellationToken);

        var solicitudItemPendiente = pendientesTop5Base
            .Select(x =>
            {
                var firmantesDeSolicitud = firmantesPendientesRelacionados
                    .Where(f => f.IdSolicitud == x.IdSolicitud && f.IdPlantilla == x.IdPlantilla)
                    .ToList();

                var ultimaOrdenEtapa = firmantesDeSolicitud.Count > 0
                    ? firmantesDeSolicitud.Max(f => f.OrdenEtapa)
                    : 0;

                var ultimosFirmantes = firmantesDeSolicitud
                    .Where(f => f.OrdenEtapa == ultimaOrdenEtapa)
                    .Select(f => f.Firmante)
                    .Distinct()
                    .ToList();

                return new SolicitudItemPendienteDto
                {
                    IdSolicitud = x.IdSolicitud,
                    CodigoSolicitud = x.CodigoSolicitud,
                    Asunto = x.Asunto,
                    Urgente = x.EsUrgente ? "Si" : "No",
                    Solicitante = x.Solicitante,
                    AreaSolicitante = x.AreaSolicitante,
                    UltimoFirmante = ultimosFirmantes,
                    FechaVencimiento = x.FechaLimite
                };
            })
            .ToList();

        var registradasBase = await solicitudesQuery
            .Where(s =>
                s.Estado &&
                s.IdUsuarioSolicitante == codigoUsuario.Value &&
                s.IdEstadoSolicitud == estadoEnCurso)
            .Select(s => new
            {
                IdSolicitud = s.Id,
                s.CodigoSolicitud,
                TipoSolicitud = s.TipoSolicitud.NombreTipoSolicitud,
                SubTipoOperacion = s.SubTipoSolicitud.NombreSubTipoSolicitud,
                TipoOperacion = s.TipoOperacion.NombreTipoOperacion,
                s.EsUrgente,
                s.FechaCreacion,
                FechaLimite = s.FechaLimite != null
                    ? s.FechaLimite.Valor
                    : (DateTime?)null,
                s.SolicitudPlantilla.IdPlantilla
            })
            .ToListAsync(cancellationToken);

        var totalRegistradas = registradasBase.Count;

        var fechaLimiteParaRechazo = DateTime.Now.AddDays(-7);

        var totalRechazadas = await solicitudesQuery.CountAsync(s =>
            s.Estado &&
            s.IdUsuarioSolicitante == codigoUsuario.Value &&
            s.IdEstadoSolicitud == estadoRechazado &&
            s.FechaAccion.Date >= fechaLimiteParaRechazo.Date,
            cancellationToken);

        var registradasTop5Base = registradasBase
            .OrderByDescending(x => x.EsUrgente ? 1 : 0)
            .ThenBy(x => x.EsUrgente ? (x.FechaLimite ?? DateTime.MaxValue) : DateTime.MaxValue)
            .ThenBy(x => !x.EsUrgente ? x.FechaCreacion : DateTime.MaxValue)
            .Take(5)
            .ToList();

        var registradasIds = registradasTop5Base
            .Select(x => x.IdSolicitud)
            .Distinct()
            .ToList();

        var registradasPlantillasIds = registradasTop5Base
            .Select(x => x.IdPlantilla)
            .Distinct()
            .ToList();

        var firmantesRegistradosRelacionados = await solicitudFirmanteFlujoQuery
            .Where(x =>
                x.Estado &&
                registradasIds.Contains(x.SolicitudPlantilla.Solicitud.Id) &&
                registradasPlantillasIds.Contains(x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id))
            .Select(x => new
            {
                IdSolicitud = x.SolicitudPlantilla.Solicitud.Id,
                IdPlantilla = x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id,
                x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa,
                x.RevAprobado,
                x.FechaRevision,
                Firmante = (
                    (x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                        ? x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                            0,
                            x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                        : x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                    + " " +
                    (x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                ).Trim()
            })
            .ToListAsync(cancellationToken);

        var solicitudItemRegistrada = registradasTop5Base
            .Select(x =>
            {
                var firmantesDeSolicitud = firmantesRegistradosRelacionados
                    .Where(f => f.IdSolicitud == x.IdSolicitud && f.IdPlantilla == x.IdPlantilla)
                    .ToList();

                var etapaActual = firmantesDeSolicitud
                    .Where(f => !f.RevAprobado)
                    .Select(f => f.OrdenEtapa)
                    .DefaultIfEmpty(0)
                    .Min();

                var firmanteSiguiente = firmantesDeSolicitud
                    .Where(f =>
                        !f.RevAprobado &&
                        f.OrdenEtapa == etapaActual)
                    .OrderBy(f => f.Firmante)
                    .Select(f => f.Firmante)
                    .FirstOrDefault();

                return new SolicitudItemRegistradaDto
                {
                    IdSolicitud = x.IdSolicitud,
                    CodigoSolicitud = x.CodigoSolicitud,
                    TipoSolicitud = x.TipoSolicitud,
                    SubTipoOperacion = x.SubTipoOperacion,
                    TipoOperacion = x.TipoOperacion,
                    Urgente = x.EsUrgente ? "Si" : "No",
                    FirmanteSiguiente = firmanteSiguiente ?? string.Empty,
                    FechaRegistro = x.FechaCreacion
                };
            })
            .ToList();

        return new SolicitudPanelControlDto
        {
            SolicitudPendiente = new SolicitudPendienteDto
            {
                Pendientes = totalPendientes,
                Urgente = totalUrgentesPendientes
            },
            Registradas = totalRegistradas,
            Rechazadas = totalRechazadas,
            SolicitudItemPendiente = solicitudItemPendiente,
            SolicitudItemRegistrada = solicitudItemRegistrada
        };
    }



    #region PAGINADO

    public async Task<SolicitudDto?> ObtenerSolicitudPorId(long id, long? idUsuario, bool esReuso, CancellationToken cancellationToken, bool tracking = false)
    {
        var solicitud = await ObtenerSolicitudBase(
            id,
            tracking,
            esReuso,
            cancellationToken);

        if (solicitud is null)
            return null;

        if (!esReuso)
        {
            await CargarInformacionDocumentos(
                 solicitud,
                 id,
                 idUsuario,
                 tracking,
                 cancellationToken);
        }
        else
        {
            solicitud.Documentos = [];
        }

        solicitud.HistorialCambios = await ObtenerHistorialEventos(
           id,
           solicitud.MotivoRechazo,
           tracking,
           cancellationToken);

        solicitud.EstadoFlujo = await ObtenerEstadoFlujo(
            id,
            idUsuario,
            tracking,
            cancellationToken);

        return solicitud;
    }

    #endregion

    #region METODOS PRIVADOS

    private async Task<SolicitudDto?> ObtenerSolicitudBase(
    long id,
    bool tracking,
    bool esReuso,
    CancellationToken cancellationToken)
    {
        var query = _context.Set<Solicitud>()
            .Where(x => x.Id == id);

        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Select(s => new SolicitudDto
            {
                IdSolicitud = s.Id,
                IdTipoSolicitud = s.IdTipoSolicitud,
                IdSubTipoSolicitud = s.IdSubTipoSolicitud,
                IdtipoOperacion = s.IdTipoOperacion,
                IdEstado = s.EstadoSolicitud.Codigo,
                Solicitante =
            ((s.UsuarioSolicitante.Nombres ?? "") + " " +
             (s.UsuarioSolicitante.ApellidoPaterno ?? "") + " " +
             (s.UsuarioSolicitante.ApellidoMaterno ?? "")).Trim(),
                CodigoSolicitud = s.CodigoSolicitud,
                Asunto = s.Asunto,
                Mensaje = s.Mensaje,
                EsUrgente = s.EsUrgente,
                InCodigo = (esReuso ? false : s.InCodigo),
                Estado = s.EstadoSolicitud.Nombre,
                FechaLimite = s.FechaLimite != null ? s.FechaLimite.Valor : null,
                FechaCreacion = s.FechaCreacion,
                MotivoRechazo = s.MotivoRechazo,

                Documentos = s.Documentos
                    .Where(d => d.Estado)
                    .Select(d => new ArchivoDto
                    {
                        Id = d.Id,
                        Nombre = (s.InCodigo ? d.NombreCodigo : d.NombreOriginal),
                        NombreOriginal = d.NombreOriginal,
                        Peso = d.PesoDocumento,
                        Extension = d.ExtensionOriginal,

                        RutaOriginal = d.RutaOriginal,
                        ExtensionOriginal = d.ExtensionOriginal,

                        RutaCodigo = d.RutaCodigo,
                        NombreCodigo = d.NombreCodigo,
                        ExtensionCodigo = d.ExtensionCodigo,

                        RutaConTag = d.RutaConTag,
                        NombreConTag = d.NombreConTag,
                        ExtensionConTag = d.ExtensionConTag,

                        InCodigo = s.InCodigo,

                        Tags = d.DocumentoTags
                                .Where(dt => dt.Estado)
                                .Select(dt => new TagsDto
                                {
                                    Id = dt.Id,
                                    NumeroPagina = dt.NumeroPagina,
                                    X = dt.UbicacionX,
                                    Y = dt.UbicacionY
                                }).ToList(),

                        RutaArchivo = (s.InCodigo ? (
                        d.RutaCodigo + "\\" +
                        d.NombreCodigo + "." +
                        d.ExtensionCodigo) : (
                        d.RutaOriginal + "\\" +
                        d.NombreOriginal + "." +
                        d.ExtensionOriginal))

                    }).ToList(),

                Sustentos = (esReuso ? new List<SustentoDto>() : s.Sustentos
                    .Where(st => st.Estado)
                    .Select(st => new SustentoDto
                    {
                        Id = st.Id,
                        Nombre = st.NombreSustento,
                        Peso = st.PesoSustento,
                        Extension = st.ExtensionSustento
                    }).ToList()),

                CodigosReferencia = s.CodigosReferencia
                    .Where(cr => cr.Estado)
                    .Select(cr => new CodigosReferenciaDto
                    {
                        Id = cr.Id,
                        Nombre = cr.CodigoReferencia.Valor,
                    }).ToList(),

                Comentario = s.Comentarios
                    .Select(c => new ComentarioDto
                    {
                        Nombre = c.Usuario.Nombres + " " +
                                 c.Usuario.ApellidoPaterno + " " +
                                 c.Usuario.ApellidoMaterno,
                        Comentario = c.Comentario,
                        fecha = c.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                    }).ToList(),

                SolicitudCorreo = s.Correos
                .Where(cr => cr.Estado)
                .Select(cr => new CorreoDto
                {
                    Id = cr.Id,
                    Nombre = cr.Correo.Value,
                }).ToList(),

                EstadoFlujo = new EstadoFlujov2Dto(),
                HistorialCambios = new List<HistorialCambiosDto>()

            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task CargarInformacionDocumentos(
    SolicitudDto solicitud,
    long idSolicitud,
    long? idUsuario,
    bool tracking,
    CancellationToken cancellationToken)
    {
        var documentosIds = solicitud.Documentos
            .Select(d => d.Id)
            .ToList();

        if (documentosIds.Count == 0)
            return;

        var query = _context.Set<DetalleFirma>()
            .Where(df =>
                df.Estado &&
                documentosIds.Contains(df.IdSolicitudDocumento) &&
                df.SolicitudDocumento.Solicitud.Id == idSolicitud);

        if (!tracking)
            query = query.AsNoTracking();


        var documentosInfo = await query
            .Select(df => new DocumentoFirmaInfo
            {
                Id = df.Id,
                IdSolicitudDocumento = df.IdSolicitudDocumento,

                // new
                IdUsuarioAsignado = df.SolicitudFirmanteFlujo.FirmanteFlujo.IdUsuarioAsignado,
                FechaHoraFirma = df.FechaHoraFirma,
                EstadoDocumento = df.EstadoDocumento,
                // new

                NombreOriginal = df.SolicitudDocumento.NombreOriginal,
                InCodigo = df.SolicitudDocumento.Solicitud.InCodigo,

                RutaCodigo = df.SolicitudDocumento.RutaCodigo,
                NombreCodigo = df.SolicitudDocumento.NombreCodigo,
                ExtensionCodigo = df.SolicitudDocumento.ExtensionCodigo,

                RutaOriginal = df.SolicitudDocumento.RutaOriginal,
                NombreDocOriginal = df.SolicitudDocumento.NombreOriginal,
                ExtensionOriginal = df.SolicitudDocumento.ExtensionOriginal,

                RutaConTag = df.SolicitudDocumento.RutaConTag,
                NombreConTag = df.SolicitudDocumento.NombreConTag,
                ExtensionConTag = df.SolicitudDocumento.ExtensionConTag,

                IdTipoAprobador = df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.IdTipoAprobador,
                ExisteFirmadoFlag = _context.Set<DetalleFirma>()
                        .Where(dd =>
                            dd.IdSolicitudDocumento == df.IdSolicitudDocumento &&
                            dd.FechaHoraFirma != null &&
                            dd.NombreDocumentoFirmado != null)
                        .Select(dd => 1)
                        .FirstOrDefault(),

                NombreDocumentoFirmado = df.NombreDocumentoFirmado,
                ExtensionDocumentoFirmado = df.ExtensionDocumentoFirmado,
                RutaDocumentoFirmado = df.RutaDocumentoFirmado,
            })
            .ToListAsync(cancellationToken);

        var agrupado = documentosInfo
            .GroupBy(x => x.IdSolicitudDocumento)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var documento in solicitud.Documentos)
        {
            if (!agrupado.TryGetValue(documento.Id, out var firmas))
            {
                InicializarDocumentoSinFirma(documento);
                continue;
            }


            ActualizarDocumento(
                documento,
                firmas,
                idUsuario);
        }
    }

    private static void InicializarDocumentoSinFirma(
    ArchivoDto documento)
    {
        documento.FirmadoPorMi = false;
        documento.FirmadoPorOtro = false;
        documento.Editar = true;

        if (!string.IsNullOrWhiteSpace(documento.RutaConTag))
        {
            documento.Nombre = documento.NombreConTag;
            documento.Extension = documento.ExtensionConTag;
            documento.RutaArchivo =
                $"{documento.RutaConTag}\\{documento.NombreConTag}.{documento.ExtensionConTag}";
            return;
        }

        if (documento.InCodigo)
        {
            documento.Nombre = documento.NombreCodigo;
            documento.Extension = documento.ExtensionCodigo;
            documento.RutaArchivo =
                $"{documento.RutaCodigo}\\{documento.NombreCodigo}.{documento.ExtensionCodigo}";
            return;
        }

        documento.Nombre = documento.NombreOriginal;
        documento.Extension = documento.ExtensionOriginal;
        documento.RutaArchivo =
            $"{documento.RutaOriginal}\\{documento.NombreOriginal}.{documento.ExtensionOriginal}";
    }

    private static void ActualizarDocumento(
    ArchivoDto documento,
    List<DocumentoFirmaInfo> firmas,
    long? idUsuario)
    {
        var firmaDelUsuario = idUsuario.HasValue
            ? firmas.FirstOrDefault(x => x.IdUsuarioAsignado == idUsuario.Value)
            : null;

        documento.FirmadoPorMi =
            firmaDelUsuario?.FechaHoraFirma.HasValue == true;

        documento.FirmadoPorOtro =
            firmas.Any(f =>
                f.IdUsuarioAsignado != idUsuario &&
                f.FechaHoraFirma.HasValue &&
                !string.IsNullOrEmpty(f.NombreDocumentoFirmado));

        documento.Editar =
            firmaDelUsuario == null ||
            (firmaDelUsuario.EstadoDocumento != VO.EstadoDocumento.Bloqueado &&
             firmaDelUsuario.EstadoDocumento != VO.EstadoDocumento.Firmado);

        var referencia = firmaDelUsuario ?? firmas.First();

        documento.IdDetalleFirma = referencia.Id;
        documento.NombreOriginal = referencia.NombreOriginal;
        documento.IdTipoAprobador = referencia.IdTipoAprobador;

        AsignarArchivoDocumento(
            documento,
            referencia,
            firmas);
    }

    private static void AsignarArchivoDocumento(
    ArchivoDto documento,
    DocumentoFirmaInfo referencia,
    List<DocumentoFirmaInfo> firmas)
    {
        var documentoFirmado = firmas
            .Where(f =>
                f.FechaHoraFirma.HasValue &&
                !string.IsNullOrWhiteSpace(f.NombreDocumentoFirmado))
            .OrderByDescending(f => f.NombreDocumentoFirmado!.Length)
            .FirstOrDefault();

        if (documentoFirmado != null)
        {
            documento.Nombre = documentoFirmado.NombreDocumentoFirmado;
            documento.Extension = documentoFirmado.ExtensionDocumentoFirmado;
            documento.RutaArchivo =
                $"{documentoFirmado.RutaDocumentoFirmado}\\{documentoFirmado.NombreDocumentoFirmado}.{documentoFirmado.ExtensionDocumentoFirmado}";
            return;
        }

        if (!string.IsNullOrWhiteSpace(referencia.RutaConTag))
        {
            documento.Nombre = referencia.NombreConTag;
            documento.Extension = referencia.ExtensionConTag;
            documento.RutaArchivo =
                $"{referencia.RutaConTag}\\{referencia.NombreConTag}.{referencia.ExtensionConTag}";
            return;
        }

        if (referencia.InCodigo)
        {
            documento.Nombre = referencia.NombreCodigo;
            documento.Extension = referencia.ExtensionCodigo;
            documento.RutaArchivo =
                $"{referencia.RutaCodigo}\\{referencia.NombreCodigo}.{referencia.ExtensionCodigo}";
            return;
        }

        documento.Nombre = referencia.NombreDocOriginal;
        documento.Extension = referencia.ExtensionOriginal;
        documento.RutaArchivo =
            $"{referencia.RutaOriginal}\\{referencia.NombreDocOriginal}.{referencia.ExtensionOriginal}";
    }

    private async Task<List<HistorialCambiosDto>> ObtenerHistorialEventos(
    long idSolicitud,
    string? motivoRechazo,
    bool tracking,
    CancellationToken cancellationToken)
    {
        IQueryable<SolicitudEvento> queryHistorialEvento = _context.Set<SolicitudEvento>()
             .Include(m => m.TipoEvento)
             .Include(m => m.PlantillaEtapa)
             .Include(m => m.SolicitudSustento)
             .Where(m => m.IdSolicitud == idSolicitud);
        if (!tracking)
            queryHistorialEvento = queryHistorialEvento.AsNoTracking();


        var eventos = await queryHistorialEvento
        .Select(m => new
        {
            m.TipoEvento.Id,
            Template = m.TipoEvento.FormatoMensaje,
            Fecha = m.FechaEvento,

            UsuarioNombre = m.UsuarioOrigen.Nombres,
            UsuarioApellidoPaterno = m.UsuarioOrigen.ApellidoPaterno,

            UsuarioAgregadoNombre = m.UsuarioDestino != null
                ? m.UsuarioDestino.Nombres
                : null,
            UsuarioAgregadoApellidoPaterno = m.UsuarioDestino != null
                ? m.UsuarioDestino.ApellidoPaterno
                : null,

            NombreEtapa = m.PlantillaEtapa != null ? m.PlantillaEtapa.NombreEtapa : null,
            NombreArea = m.AreaUsuario,
            NombreSustento = m.SolicitudSustento.NombreSustento + "." + m.SolicitudSustento.ExtensionSustento
        }).OrderByDescending(m => m.Fecha)
        .ToListAsync(cancellationToken);


        var resultadoTipoEvento = eventos.Select(e =>
        {
            var model = new
            {
                usuario = ObtenerPrimerNombreYApellido(e.UsuarioNombre, e.UsuarioApellidoPaterno),
                usuario_agregado = ObtenerPrimerNombreYApellido(e.UsuarioAgregadoNombre, e.UsuarioAgregadoApellidoPaterno),
                usuario_retirado = ObtenerPrimerNombreYApellido(e.UsuarioAgregadoNombre, e.UsuarioAgregadoApellidoPaterno),
                usuario_delegado = ObtenerPrimerNombreYApellido(e.UsuarioAgregadoNombre, e.UsuarioAgregadoApellidoPaterno),
                nombre_etapa = e.NombreEtapa,
                nombre_area = e.NombreArea,
                motivo_rechazo = motivoRechazo,
                nombre_sustento = e.NombreSustento
            };

            var comentario = _htmlTemplateRenderer.Render(e.Template, model);

            return new HistorialCambiosDto
            {
                Nombre = ObtenerPrimerNombreYApellido(e.UsuarioNombre, e.UsuarioApellidoPaterno),
                Comentario = comentario,
                fecha = e.Fecha.Value.ToString("dd/MM/yyyy HH:mm")
            };
        });

        return resultadoTipoEvento.ToList();
    }

    [SuppressMessage(
       "Major Code Smell",
       "S3776:Cognitive Complexity of methods should not be too high",
       Justification = "Consulta compleja de para obtener conteo de solicitud para firma. Se mantiene temporalmente por compatibilidad con EF Core.")]
    private async Task<EstadoFlujov2Dto> ObtenerEstadoFlujo(
    long idSolicitud,
    long? idUsuario,
    bool tracking,
    CancellationToken cancellationToken)
    {
        var queryEstadoFlujo = _context.Set<SolicitudPlantilla>()
           .Where(sp => sp.IdSolicitud == idSolicitud);

        if (!tracking)
            queryEstadoFlujo = queryEstadoFlujo.AsNoTracking();

        var estadoFlujoRaw = await queryEstadoFlujo
            .SelectMany(sp => sp.Plantilla.Etapas.Where(e => e.Estado).SelectMany(e => e.Firmantes.Where(e => e.Estado).Select(f => new
            {
                IdPlantilla = sp.Plantilla.Id,
                NombrePlantilla = sp.Plantilla.NombrePlantilla!.Valor,
                DescripcionPlantilla = sp.Plantilla.Descripcion!.Valor,
                IdEtapa = e.Id,
                e.OrdenEtapa,
                NombreEtapa = e.NombreEtapa.Valor,
                e.IdTipoEtapa,
                TipoEtapa = e.ParametroTipoEtapa.ValorParametro,
                IdtipoAprobador = e.IdTipoAprobador,
                TipoAprobador = e.ParametroTipoAprobador.ValorParametro,
                IdFirmante = f.Id,
                f.IdUsuarioAsignado,
                f.FirmanteFlujoUsuarioAsignado.Nombres,
                f.FirmanteFlujoUsuarioAsignado.ApellidoPaterno,
                f.FirmanteFlujoUsuarioAsignado.ApellidoMaterno,
                f.CargoUsuario,
                f.AreaUsuario,
                DepartamentoUsuario =
                        f.FirmanteFlujoUsuarioAsignado.Area != null &&
                        f.FirmanteFlujoUsuarioAsignado.Area.AreaPadre != null
                            ? f.FirmanteFlujoUsuarioAsignado.Area.AreaPadre.NombreArea
                            : string.Empty,
                f.GerenciaUsuario,
                Estado = sp.FirmantesFlujo
                    .Where(sf => sf.IdPlantillaEtapa == e.Id && sf.IdFirmanteFlujo == f.Id)
                    .Select(sf => sf.RevAprobado)
                    .FirstOrDefault(),
                FechaRevision = sp.FirmantesFlujo
                    .Where(sf => sf.IdPlantillaEtapa == e.Id && sf.IdFirmanteFlujo == f.Id)
                    .Select(sf => sf.FechaRevision)
                    .FirstOrDefault(),
                IdSolicitudFirmanteFlujo = sp.FirmantesFlujo
                    .Where(sf => sf.IdPlantillaEtapa == e.Id &&
                                 sf.IdFirmanteFlujo == f.Id)
                    .Select(sf => sf.Id)
                    .FirstOrDefault()
            })))
            .ToListAsync(cancellationToken);

        var idsFirmados = await _context.Set<DetalleFirma>()
            .Where(x =>
                x.Estado &&
                x.EstadoDocumento == VO.EstadoDocumento.Firmado &&
                x.SolicitudDocumento.IdSolicitud == idSolicitud)
            .Select(x => x.IdSolicitudFirmanteFlujo)
            .Distinct()
            .ToHashSetAsync(cancellationToken);

        var ordenEtapaActual = estadoFlujoRaw
            .Where(x => !x.Estado)
            .OrderBy(x => x.OrdenEtapa)
            .Select(x => (long?)x.OrdenEtapa)
            .FirstOrDefault();


        var estadoFlujo = estadoFlujoRaw
            .GroupBy(x => new
            {
                x.IdPlantilla,
                x.NombrePlantilla,
                x.DescripcionPlantilla
            })
            .Select(p => new EstadoFlujov2Dto
            {
                IdPlantilla = p.Key.IdPlantilla,
                NombrePlantilla = p.Key.NombrePlantilla,
                Descripcion = p.Key.DescripcionPlantilla,
                etapas = p
                    .GroupBy(e => new
                    {
                        e.IdEtapa,
                        e.OrdenEtapa,
                        e.NombreEtapa,
                        e.IdTipoEtapa,
                        e.TipoEtapa,
                        e.IdtipoAprobador,
                        e.TipoAprobador
                    })
                    .OrderBy(e => e.Key.OrdenEtapa)
                    .Select(e => new PlantillaEtapav2Dto
                    {
                        Id = e.Key.IdEtapa,
                        OrdenEtapa = e.Key.OrdenEtapa,
                        NombreEtapa = e.Key.NombreEtapa,
                        IdTipoEtapa = e.Key.IdTipoEtapa,
                        ValorTipoEtapa = e.Key.TipoEtapa,
                        IdTipoAprobador = e.Key.IdtipoAprobador,
                        ValorTipoAprobador = e.Key.TipoAprobador,
                        Firmantes = e.Select(f =>
                        {
                            var estaAprobado = f.Estado && f.FechaRevision.HasValue;
                            var estaFirmado = idsFirmados.Contains(f.IdSolicitudFirmanteFlujo);
                            var esTuTurno =
                                ordenEtapaActual.HasValue &&
                                e.Key.OrdenEtapa == ordenEtapaActual.Value &&
                                idUsuario.HasValue &&
                                f.IdUsuarioAsignado == idUsuario.Value;

                            return new FirmanteFlujov2Dto
                            {
                                Id = f.IdFirmante,
                                IdPlantillaEtapa = e.Key.IdEtapa,
                                IdUsuarioAsignado = f.IdUsuarioAsignado,
                                NombreUsuarioAsignado = ObtenerPrimerNombreYApellido(f.Nombres, f.ApellidoPaterno),
                                Nombres = f.Nombres,
                                ApellidoPaterno = f.ApellidoPaterno,
                                ApellidoMaterno = f.ApellidoMaterno,
                                CargoUsuario = f.CargoUsuario,
                                AreaUsuario = f.AreaUsuario,
                                DepartamentoUsuario = f.DepartamentoUsuario,
                                GerenciaUsuario = f.GerenciaUsuario,
                                Estado = estaAprobado
                                    ? f.FechaRevision!.Value.ToString("dd/MM/yyyy HH:mm")
                                    : esTuTurno
                                        ? "Tu Turno"
                                        : "Pendiente",
                                Firmado = (estaAprobado || estaFirmado),
                                Eliminar = !(estaAprobado || estaFirmado)
                                 && (idUsuario.HasValue &&
                                f.IdUsuarioAsignado != idUsuario.Value)
                            };
                        }).ToList()
                    }).ToList()
            })
            .FirstOrDefault();

        return estadoFlujo ?? new EstadoFlujov2Dto();
    }


    #endregion

    #region DTO

    private sealed class DocumentoFirmaInfo
    {
        public long Id { get; set; }
        public long IdSolicitudDocumento { get; set; }
        public long IdUsuarioAsignado { get; set; }
        public DateTime? FechaHoraFirma { get; set; }
        public VO.EstadoDocumento EstadoDocumento { get; set; } = default!;
        public string NombreOriginal { get; set; } = string.Empty;
        public bool InCodigo { get; set; }
        public string? RutaCodigo { get; set; }
        public string? NombreCodigo { get; set; }
        public string? ExtensionCodigo { get; set; }
        public string? RutaOriginal { get; set; }
        public string? NombreDocOriginal { get; set; }
        public string? ExtensionOriginal { get; set; }
        public string? RutaConTag { get; set; }
        public string? NombreConTag { get; set; }
        public string? ExtensionConTag { get; set; }
        public long IdTipoAprobador { get; set; }
        public int ExisteFirmadoFlag { get; set; }
        public string? NombreDocumentoFirmado { get; set; }
        public string? ExtensionDocumentoFirmado { get; set; }
        public string? RutaDocumentoFirmado { get; set; }
    }
    #endregion
}
