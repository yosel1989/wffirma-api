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
using System.Linq.Expressions;
using VO = COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Domain.Entities;
// NOTA: agregar los using de las entidades Usuario y AccesoEspecial si viven en otro namespace.
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

// Cambios respecto a la version anterior:
//  - Acceso especial integrado (region ACCESO ESPECIAL).
//  - TimeProvider inyectado (registrar: services.AddSingleton(TimeProvider.System)).
//  - Nombre "primer nombre + apellido" se formatea en memoria (antes: 5 copias de Substring/IndexOf en LINQ).
//  - Top 5 y firmantes relacionados extraidos a helpers (antes duplicados).
//  - Rechazadas: comparacion sargable (FechaAccion >= desde) en lugar de FechaAccion.Date.
//  - Historial: sin Include redundantes y fecha null-safe.
//  - ExisteFirmadoFlag eliminado (subconsulta por fila que no se usaba).
//  - ObtenerSolicitudBase con AsSplitQuery.
public partial class SolicitudQueryRepository(
        WorkflowFirmaDbContext context,
        IHtmlTemplateRenderer htmlTemplateRenderer,
        TimeProvider timeProvider
    ) : ISolicitudQueryRepository
{

    private readonly WorkflowFirmaDbContext _context = context;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;
    private readonly TimeProvider _timeProvider = timeProvider;

    private const char EstadoAccesoActivo = 'A';

    private static string ObtenerPrimerNombreYApellido(string? nombres, string? apellidoPaterno)
    {
        var primerNombre = string.IsNullOrWhiteSpace(nombres)
            ? string.Empty
            : nombres.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                     .FirstOrDefault() ?? string.Empty;

        apellidoPaterno ??= string.Empty;

        return $"{primerNombre} {apellidoPaterno}".Trim();
    }

    private static List<T> TomarTop5Prioritarias<T>(
        IEnumerable<T> origen,
        Func<T, bool> esUrgente,
        Func<T, DateTime?> fechaLimite,
        Func<T, DateTime> fechaCreacion) =>
        origen
            .OrderByDescending(esUrgente)
            .ThenBy(x => esUrgente(x)
                ? fechaLimite(x) ?? DateTime.MaxValue
                : fechaCreacion(x))
            .Take(5)
            .ToList();

    private static async Task<List<FirmanteRelacionado>> ObtenerFirmantesRelacionados(
        IQueryable<SolicitudFirmanteFlujo> solicitudFirmanteFlujoQuery,
        List<long> idsSolicitud,
        List<long> idsPlantilla,
        CancellationToken cancellationToken)
    {
        return await solicitudFirmanteFlujoQuery
            .Where(x =>
                x.Estado &&
                idsSolicitud.Contains(x.SolicitudPlantilla.Solicitud.Id) &&
                idsPlantilla.Contains(x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id))
            .Select(x => new FirmanteRelacionado(
                x.SolicitudPlantilla.Solicitud.Id,
                x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id,
                x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa,
                x.RevAprobado,
                x.FechaRevision,
                x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres,
                x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno))
            .ToListAsync(cancellationToken);
    }

    #region ACCESO ESPECIAL

    // WFF_ACCESO_ESPECIAL: ID_USUARIO, ID_AREA (gerencia o departamento), CH_ESTADO (A=Activo, S=Suspendido).
    // WFF_SOLICITUD: ID_AREA_SOLICITUD (departamento) e ID_AREA_PADRE_SOLI (gerencia).
    // Todas las areas viven en WFF_AREA, por lo que un mismo id no es gerencia y departamento a la vez.

    /// <summary>
    /// Paso 1: valida si el usuario tiene acceso especial ACTIVO al area filtrada (gerencia o departamento).
    /// Devuelve el predicado con lo que ese acceso agrega (coordinador = todo lo generado en el area;
    /// solicitante-aprobador = solo lo suyo), o null si no hay acceso: la consulta actual queda tal cual.
    /// </summary>
    private async Task<Expression<Func<Solicitud, bool>>?> ObtenerPredicadoAccesoEspecial(
        long idUsuario,
        long? idAreaFiltro,
        CancellationToken cancellationToken)
    {
        if (!idAreaFiltro.HasValue)
            return null;

        var idArea = idAreaFiltro.Value;

        var tieneAcceso = await _context.Set<AccesoEspecial>()
            .AsNoTracking()
            .AnyAsync(a =>
                a.Estado == true &&
                a.IdUsuario == idUsuario &&
                a.IdArea == idArea,
                cancellationToken);

        if (!tieneAcceso)
            return null;

        var esCoordinador = await _context.Set<Usuario>()
            .AsNoTracking()
            .AnyAsync(u =>
                u.Id == idUsuario &&
                u.Rol == Domain.Common.Enums.RolUsuario.Coordinador,
                cancellationToken);

        var soloPropias = !esCoordinador;

        // Gerencia -> la solicitud la tiene como area padre; departamento -> como area de la solicitud.
        // El OR cubre ambos casos (y al usuario ubicado directamente en la gerencia).
        return s =>
            (s.IdAreaPadreSolicitud == idArea || s.IdAreaSolicitud == idArea)
            && (!soloPropias || s.IdUsuarioSolicitante == idUsuario);
    }

    /// <summary>
    /// Paso 2: suma los registros del acceso especial a la consulta actual de "Mis Solicitudes".
    /// Se usa IN sobre ids (no UNION de entidades) porque WFF_SOLICITUD tiene un CLOB (CL_MENSAJE)
    /// y en Oracle UNION/DISTINCT sobre CLOB falla con ORA-00932.
    /// Aplicar en la etapa de VISIBILIDAD, antes de los demas filtros (estado, urgencia, texto, fechas).
    /// Pendiente: invocarlo desde el listado paginado de "Mis Solicitudes" (no esta en este archivo):
    ///   query = await AgregarAccesoEspecial(query, idUsuario, idAreaFiltro, tracking, ct);
    /// </summary>
    private async Task<IQueryable<Solicitud>> AgregarAccesoEspecial(
        IQueryable<Solicitud> consultaActual,
        long idUsuario,
        long? idAreaFiltro,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var predicado = await ObtenerPredicadoAccesoEspecial(idUsuario, idAreaFiltro, cancellationToken);

        if (predicado is null)
            return consultaActual;

        IQueryable<Solicitud> origen = tracking
            ? _context.Set<Solicitud>()
            : _context.Set<Solicitud>().AsNoTracking();

        var idsActuales = consultaActual.Select(s => s.Id);
        var idsAcceso = origen
            .Where(s => s.Estado)
            .Where(predicado)
            .Select(s => s.Id);

        return origen.Where(s => idsActuales.Contains(s.Id) || idsAcceso.Contains(s.Id));
    }

    #endregion

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

        // Se proyectan Nombres y ApellidoPaterno crudos; el formato "primer nombre + apellido" se aplica en memoria.
        var pendientesConDocumentos = await queryConDocumentos
            .Select(df => new
            {
                IdSolicitud = df.SolicitudDocumento.Solicitud.Id,
                df.SolicitudDocumento.Solicitud.CodigoSolicitud,
                df.SolicitudDocumento.Solicitud.Asunto,
                df.SolicitudDocumento.Solicitud.EsUrgente,
                SolicitanteNombres = df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres,
                SolicitanteApellido = df.SolicitudDocumento.Solicitud.UsuarioSolicitante.ApellidoPaterno,
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
                SolicitanteNombres = sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres,
                SolicitanteApellido = sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.ApellidoPaterno,
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
                x.SolicitanteNombres,
                x.SolicitanteApellido,
                x.AreaSolicitante,
                x.FechaCreacion,
                x.FechaLimite,
                x.IdPlantilla
            })
            .Select(g => g.First())
            .ToList();

        var totalPendientes = pendientesBase.Count;
        var totalUrgentesPendientes = pendientesBase.Count(x => x.EsUrgente);

        var pendientesTop5Base = TomarTop5Prioritarias(
            pendientesBase,
            x => x.EsUrgente,
            x => x.FechaLimite,
            x => x.FechaCreacion);

        var firmantesPendientesRelacionados = await ObtenerFirmantesRelacionados(
            solicitudFirmanteFlujoQuery,
            pendientesTop5Base.Select(x => x.IdSolicitud).Distinct().ToList(),
            pendientesTop5Base.Select(x => x.IdPlantilla).Distinct().ToList(),
            cancellationToken);

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
                    .Select(f => ObtenerPrimerNombreYApellido(f.Nombres, f.ApellidoPaterno))
                    .Distinct()
                    .ToList();

                return new SolicitudItemPendienteDto
                {
                    IdSolicitud = x.IdSolicitud,
                    CodigoSolicitud = x.CodigoSolicitud,
                    Asunto = x.Asunto,
                    Urgente = x.EsUrgente ? "Si" : "No",
                    Solicitante = ObtenerPrimerNombreYApellido(x.SolicitanteNombres, x.SolicitanteApellido),
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

        // Comparacion sargable: permite usar indices sobre FE_FECHA_ACCION.
        var desdeRechazo = _timeProvider.GetLocalNow().Date.AddDays(-7);

        var totalRechazadas = await solicitudesQuery.CountAsync(s =>
            s.Estado &&
            s.IdUsuarioSolicitante == codigoUsuario.Value &&
            s.IdEstadoSolicitud == estadoRechazado &&
            s.FechaAccion >= desdeRechazo,
            cancellationToken);

        var registradasTop5Base = TomarTop5Prioritarias(
            registradasBase,
            x => x.EsUrgente,
            x => x.FechaLimite,
            x => x.FechaCreacion);

        var firmantesRegistradosRelacionados = await ObtenerFirmantesRelacionados(
            solicitudFirmanteFlujoQuery,
            registradasTop5Base.Select(x => x.IdSolicitud).Distinct().ToList(),
            registradasTop5Base.Select(x => x.IdPlantilla).Distinct().ToList(),
            cancellationToken);

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
                    .Select(f => ObtenerPrimerNombreYApellido(f.Nombres, f.ApellidoPaterno))
                    .OrderBy(nombre => nombre)
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

        // AsSplitQuery: la proyeccion trae varias colecciones (documentos, sustentos, comentarios, etc.);
        // en una sola consulta generan un producto cartesiano.
        return await query
            .AsSplitQuery()
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

                IdUsuarioAsignado = df.SolicitudFirmanteFlujo.FirmanteFlujo.IdUsuarioAsignado,
                FechaHoraFirma = df.FechaHoraFirma,
                EstadoDocumento = df.EstadoDocumento,

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
        // Sin Include: con Select/proyeccion EF ya trae solo lo proyectado.
        IQueryable<SolicitudEvento> queryHistorialEvento = _context.Set<SolicitudEvento>()
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
                fecha = e.Fecha?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty
            };
        });

        return resultadoTipoEvento.ToList();
    }

    // PENDIENTE DE DECISION: FirstOrDefault() al final solo devuelve la primera plantilla de la solicitud.
    // Si una solicitud puede tener mas de una plantilla, EstadoFlujov2Dto debe pasar a ser una lista.
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

    // Los tipos de OrdenEtapa/ids se asumen long; ajustar si en el modelo son int.
    private sealed record FirmanteRelacionado(
        long IdSolicitud,
        long IdPlantilla,
        long OrdenEtapa,
        bool RevAprobado,
        DateTime? FechaRevision,
        string? Nombres,
        string? ApellidoPaterno);

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
        public string? NombreDocumentoFirmado { get; set; }
        public string? ExtensionDocumentoFirmado { get; set; }
        public string? RutaDocumentoFirmado { get; set; }
    }
    #endregion
}