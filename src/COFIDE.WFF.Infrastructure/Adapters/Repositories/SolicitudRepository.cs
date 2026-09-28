using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Pageds;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using VO = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class SolicitudRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<Solicitud, long>(context), ISolicitudRepository, ISolicitudReadPort
{

    public async Task<long> ObtenerSiguienteIdSolFirmanteAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<SolicitudFirmanteFlujo>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdSolFirmanteAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<SolicitudFirmanteFlujo>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>();

        if (cantidad <= 0)
            return result;

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<int> ContarListadoSolicitud(CancellationToken cancellationToken)
    {
        return await _context.Set<Solicitud>()
            .AsNoTracking()
            .CountAsync(cancellationToken);
    }

    #region Aprobacion

    public async Task<Solicitud?> ObtenerParaAprobacion(long id, CancellationToken cancellationToken)
    {
        return await _context.Solicitudes
            .Include(s => s.Documentos.Where(sd => sd.Estado))
            .ThenInclude(d => d.DetallesFirma.Where(df => df.Estado))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
    #endregion

    public async Task<FormatoNotificacionSolicitud?> ObtenerParaNotificaciones(long id, CancellationToken cancellationToken)
    {
        var existe = await _context.SolicitudDocumentos
             .CountAsync(
                 d => d.IdSolicitud == id && d.Estado,
                 cancellationToken) > 0;

        var solicitudes = await _context.Solicitudes
             .AsNoTracking()
             .Include(s => s.Documentos.Where(sd => sd.Estado))
             .Include(s => s.SolicitudPlantilla)
             .Where(x => x.Id == id)
             .Select(x => new FormatoNotificacionSolicitud(
                 x.TipoSolicitud.NombreTipoSolicitud,
                 x.CodigoSolicitud,
                 x.UsuarioSolicitante.Email,
                 x.UsuarioSolicitante.Nombres + " " + x.UsuarioSolicitante.ApellidoPaterno + " " + x.UsuarioSolicitante.ApellidoMaterno ?? "",
                 x.SubTipoSolicitud.NombreSubTipoSolicitud,
                 x.TipoOperacion.NombreTipoOperacion,
                 x.Asunto!,
                 x.FechaCreacion.ToString("dd/MM/yyyy"),
                 x.EsUrgente,
                 x.FechaLimite!.Valor,
                 x.MotivoRechazo,
                 x.SolicitudPlantilla.Id,
                 existe
             ))
             .FirstOrDefaultAsync(cancellationToken);

        return solicitudes;
    }


    public async Task<Solicitud?> ObtenerSolicitudParaRevision(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<Solicitud> query = _context.Set<Solicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(s => s.SolicitudPlantilla)
               .ThenInclude(s => s.FirmantesFlujo)
            .Include(s => s.Documentos)
                .ThenInclude(s => s.DetallesFirma)
            .Include(s => s.Comentarios);

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    // Este método SOLO se usará para validar los permisos de aprobaciones
    public async Task<Solicitud?> ObtenerSolicitudParaAprobacion(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<Solicitud> query = _context.Set<Solicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(s => s.SolicitudPlantilla)
               .ThenInclude(s => s.FirmantesFlujo)
            .Include(s => s.Documentos.Where(sd => sd.Estado))
                .ThenInclude(s => s.DetallesFirma.Where(sd => sd.Estado))
            .Include(s => s.Comentarios);

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    // Este método SOLO se usará para validar los permisos de rechazao
    public async Task<Solicitud?> ObtenerSolicitudParaRechazo(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<Solicitud> query = _context.Set<Solicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(s => s.SolicitudPlantilla)
                .ThenInclude(s => s.FirmantesFlujo)
            .Include(s => s.Documentos.Where(sd => sd.Estado))
                .ThenInclude(s => s.DetallesFirma.Where(sd => sd.Estado))
            .Include(s => s.Comentarios);

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    // Obtener el agregado de la solicitud
    public async Task<Solicitud?> ObtenerAgregadoSolicitudAsync(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<Solicitud> query = _context.Set<Solicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(s => s.SolicitudPlantilla)
                .ThenInclude(s => s.FirmantesFlujo)
            .Include(s => s.Documentos.Where(sd => sd.Estado))
                .ThenInclude(d => d.DetallesFirma.Where(sd => sd.Estado))
            .Include(s => s.Documentos.Where(sd => sd.Estado))
                .ThenInclude(d => d.DocumentoTags.Where(sd => sd.Estado))
            .Include(s => s.Sustentos.Where(sd => sd.Estado))
            .Include(s => s.CodigosReferencia)
            .Include(s => s.Correos)
            .Include(s => s.Comentarios);

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Solicitud?> ObtenerSolicitudConDocumentosYSustentosPorIdAsync(
        long id,
        List<long>? idsDocumentos,
        List<long>? idsSustentos,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        IQueryable<Solicitud> query = _context.Solicitudes
            .Where(x => x.Id == id)
            .Include(x => x.Documentos
                .Where(d =>
                    idsDocumentos == null ||
                    (idsDocumentos.Count == 0) ||
                    idsDocumentos.Contains(d.Id)))
                .ThenInclude(d => d.DetallesFirma.Where(xd => xd.Estado))
            .Include(x => x.Sustentos
                .Where(s =>
                    idsSustentos == null ||
                    (idsSustentos.Count == 0) ||
                    idsSustentos.Contains(s.Id)));

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }


    #region CONTEXT

    private static EntityState ObtenerEstado(bool esNuevo)
    {
        return esNuevo
            ? EntityState.Added
            : EntityState.Modified;
    }

    private void MarcarCodigosReferencia(Solicitud solicitud)
    {
        foreach (var codigo in solicitud.CodigosReferencia)
        {
            var estado = ObtenerEstado(codigo.EsNuevo);
            _context.Entry(codigo).State = estado;
            MarcarOwned(codigo, x => x.CodigoReferencia, estado);
        }
    }

    private void MarcarCorreos(Solicitud solicitud)
    {
        foreach (var correo in solicitud.Correos)
        {
            var estado = ObtenerEstado(correo.EsNuevo);
            _context.Entry(correo).State = estado;
            MarcarOwned(correo, x => x.Correo, estado);
        }
    }

    private void MarcarSustentos(Solicitud solicitud)
    {
        foreach (var sustento in solicitud.Sustentos)
        {
            _context.Entry(sustento).State =
                sustento.EsNuevo ? EntityState.Added : EntityState.Modified;
        }
    }

    private void MarcarEventos(Solicitud solicitud)
    {
        foreach (var evento in solicitud.Eventos)
        {
            _context.Entry(evento).State =
                evento.EsNuevo
                    ? EntityState.Added
                    : EntityState.Modified;
        }
    }

    private void MarcarComentarios(Solicitud solicitud)
    {
        foreach (var coemntario in solicitud.Comentarios)
        {
            _context.Entry(coemntario).State =
                coemntario.EsNuevo
                    ? EntityState.Added
                    : EntityState.Modified;
        }
    }

    private void MarcarDocumentos(Solicitud solicitud)
    {
        foreach (var documento in solicitud.Documentos)
        {

            _context.Entry(documento).State =
                documento.EsNuevo
                    ? EntityState.Added
                    : EntityState.Modified;

            foreach (var detlaleFirma in documento.DetallesFirma)
            {
                _context.Entry(detlaleFirma).State =
                    detlaleFirma.EsNuevo
                        ? EntityState.Added
                        : EntityState.Modified;
            }

            foreach (var documentoTag in documento.DocumentoTags)
            {
                _context.Entry(documentoTag).State =
                    documentoTag.EsNuevo
                        ? EntityState.Added
                        : EntityState.Modified;
            }
        }
    }

    public void GuardarFlujoSolicitudAsync(Solicitud solicitud)
    {
        var flujo = solicitud.SolicitudPlantilla;
        if (flujo != null)
        {

            _context.Entry(flujo).State = flujo.EsNuevo ? EntityState.Added : EntityState.Modified;

            foreach (var firmanteFlujo in flujo.FirmantesFlujo)
            {
                _context.Entry(firmanteFlujo).State =
                    firmanteFlujo.EsNuevo
                        ? EntityState.Added
                        : EntityState.Modified;
            }
        }

        MarcarCodigosReferencia(solicitud);
        MarcarCorreos(solicitud);
        MarcarSustentos(solicitud);
        MarcarEventos(solicitud);
        MarcarComentarios(solicitud);
        MarcarDocumentos(solicitud);
    }



    private void MarcarOwned<TEntity, TOwned>(
        TEntity entity,
        Expression<Func<TEntity, TOwned?>> navigation,
        EntityState state)
        where TEntity : class
        where TOwned : class
    {
        var owned = _context.Entry(entity)
            .Reference(navigation)
            .TargetEntry;

        if (owned != null)
        {
            owned.State = state;
        }
    }

    #endregion

    #region 20260521

    public async Task<SolicitudDocumento?> ObtenerDocumentoParaFirmaInternaMasivaRead(long idSolicitud, long idSolicitudDocumento, CancellationToken cancellationToken, bool tracking = false)
    {

        var query = _context.SolicitudDocumentos
    .Include(sd => sd.Solicitud)
        .ThenInclude(s => s.SolicitudPlantilla)
            .ThenInclude(sp => sp.FirmantesFlujo.Where(spf => spf.Estado))
    .Include(sd => sd.DetallesFirma.Where(sdf => sdf.Estado))
        .ThenInclude(df => df.UsuarioFirmante)
    .Include(sd => sd.DocumentoTags)
    .Where(sd =>
        sd.Estado &&
        sd.Solicitud.Id == idSolicitud &&
        sd.Id == idSolicitudDocumento);

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SolicitudDocumento>> ObtenerDocumentosParaFirmaInternaMasivaRead(long idSolicitud, List<long> documentos, CancellationToken cancellationToken, bool tracking = false)
    {
        var query = _context.SolicitudDocumentos
            .Include(sd => sd.Solicitud)
                .ThenInclude(s => s.SolicitudPlantilla)
                    .ThenInclude(sp => sp.FirmantesFlujo.Where(spf => spf.Estado))
            .Include(sd => sd.DetallesFirma.Where(sdf => sdf.Estado))
                .ThenInclude(df => df.UsuarioFirmante)
            .Include(sd => sd.DocumentoTags)
            .Where(sd => sd.Solicitud.Id == idSolicitud &&
                         documentos.Contains(sd.Id));

        if (!tracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CodigoReferenciaExistente>> ObtenerCodigosExistentesAsync(
     long? idSolicitudExcluir,
     IReadOnlyCollection<string> codigos,
     CancellationToken cancellationToken)
    {
        var query = _context.Set<SolicitudCodReferencia>()
        .Where(x => x.Estado && codigos.Contains(x.CodigoReferencia.Valor));

        if (idSolicitudExcluir.HasValue)
        {
            query = query.Where(x => x.IdSolicitud != idSolicitudExcluir.Value);
        }

        return await query
            .Select(x => new CodigoReferenciaExistente(
                x.CodigoReferencia.Valor,
                x.Solicitud.CodigoSolicitud
            ))
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region PAGINADO

    [SuppressMessage(
       "Major Code Smell",
       "S3776:Cognitive Complexity of methods should not be too high",
       Justification = "AplicarFiltros")]
    private static IQueryable<Solicitud> AplicarFiltrosUniverso(
    IQueryable<Solicitud> query,
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
    bool? usuariodepartamento)
    {
        if (!string.IsNullOrWhiteSpace(roLUsuario))
        {
            var rol = roLUsuario.Trim().ToUpper();

            if (rol == "COORD" && areaUsuario.HasValue)
            {
                query = query.Where(s =>
                    s.UsuarioSolicitante.IdArea.HasValue &&
                    s.UsuarioSolicitante.IdArea.Value == areaUsuario.Value);
            }
            else if (rol == "SOLAPR" && codigoUsuario.HasValue)
            {
                query = query.Where(s =>
                    s.UsuarioSolicitante.Id == codigoUsuario.Value);
            }
        }

        /* if (usuario == true && codigoUsuario.HasValue)
         {
             query = query.Where(s =>
                 s.UsuarioSolicitante.Id == codigoUsuario.Value);
         }*/

        /* if (usuariodepartamento == true && departamentoUsuario.HasValue)
         {
             query = query.Where(s =>
                 s.UsuarioSolicitante.IdArea.HasValue &&
                 s.UsuarioSolicitante.IdArea.Value == departamentoUsuario.Value);
         }*/

        if (gerencia?.Count > 0)
        {
            query = query.Where(s =>
                s.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
                gerencia.Contains(s.UsuarioSolicitante.Area.IdAreaPadre.Value));
        }

        if (departamento?.Count > 0)
        {
            query = query.Where(s =>
                s.UsuarioSolicitante.IdArea.HasValue &&
                departamento.Contains(s.UsuarioSolicitante.IdArea.Value));
        }

        if (solicitante?.Count > 0)
        {
            query = query.Where(s =>
                solicitante.Contains(s.UsuarioSolicitante.Id));
        }

        if (tiposolicitud?.Count > 0)
        {
            query = query.Where(s =>
                tiposolicitud.Contains(s.IdTipoSolicitud));
        }

        if (subtiposolicitud?.Count > 0)
        {
            query = query.Where(s =>
                subtiposolicitud.Contains(s.IdSubTipoSolicitud));
        }

        if (tipoOperacion?.Count > 0)
        {
            query = query.Where(s =>
                tipoOperacion.Contains(s.IdTipoOperacion));
        }

        if (estado?.Count > 0)
        {
            query = query.Where(s =>
                estado.Contains(s.IdEstadoSolicitud));
        }

        if (aprobadorActual?.Count > 0)
        {
            query = query.Where(s =>
                s.Documentos
                    .SelectMany(d => d.DetallesFirma.Where(df => df.Estado))
                    .Any(df =>
                        df.EstadoDocumento == VO.EstadoDocumento.Pendiente &&
                        df.SolicitudFirmanteFlujo.FirmanteFlujo.Estado &&
                        df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado != null &&
                        aprobadorActual.Contains(
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id)));
        }

        if (!string.IsNullOrWhiteSpace(codigosolicitud))
        {
            var codigo = codigosolicitud.Trim();

            query = query.Where(s =>
                s.CodigoSolicitud != null &&
                s.CodigoSolicitud.Contains(codigo));
        }

        if (!string.IsNullOrWhiteSpace(asunto))
        {
            var asuntoFiltro = asunto.Trim();

            query = query.Where(s =>
                s.Asunto != null &&
                s.Asunto.Contains(asuntoFiltro));
        }

        if (!string.IsNullOrWhiteSpace(referencia))
        {
            var referenciaFiltro = referencia.Trim();

            query = query.Where(s =>
                s.CodigosReferencia.Any(r =>
                    r.CodigoReferencia.Valor.Contains(referenciaFiltro)));
        }

        /* if (urgente == true)
         {
             query = query.Where(s =>
                 s.EsUrgente);
         }*/

        if (DateTime.TryParse(fechainicio, out var fechaInicioParseada))
        {
            query = query.Where(s =>
                s.FechaSolicitud >= fechaInicioParseada.Date);
        }

        if (DateTime.TryParse(fechafin, out var fechaFinParseada))
        {
            var fechaFin = fechaFinParseada.Date.AddDays(1).AddTicks(-1);

            query = query.Where(s =>
                s.FechaSolicitud <= fechaFin);
        }

        return query;
    }

    private static IQueryable<Solicitud> AplicarFiltrosGrilla(
    IQueryable<Solicitud> query,
    long? codigoUsuario,
    long? areaUsuario,
    string? roLUsuario,
    long? departamentoUsuario,
    bool? urgente,
    bool? usuario,
    bool? usuariodepartamento)
    {

        if (usuario == true && codigoUsuario.HasValue)
        {
            query = query.Where(s =>
                s.UsuarioSolicitante.Id == codigoUsuario.Value);
        }

        if (usuariodepartamento == true && departamentoUsuario.HasValue)
        {
            query = query.Where(s =>
                s.UsuarioSolicitante.IdArea.HasValue &&
                s.UsuarioSolicitante.IdArea.Value == departamentoUsuario.Value);
        }

        if (urgente == true)
        {
            query = query.Where(s =>
                s.EsUrgente);
        }

        return query;
    }

    private static async Task<SolicitudResumenPaginadoDto> ObtenerResumenAsync(
    IQueryable<Solicitud> query,
    long? codigoUsuario,
    long? departamentoUsuario,
    CancellationToken cancellationToken)
    {
        return await query
            .GroupBy(_ => 1)
            .Select(g => new SolicitudResumenPaginadoDto
            {
                Total = g.LongCount(),

                Urgente = g.LongCount(x => x.EsUrgente),

                MisSolicitudes = codigoUsuario.HasValue
                    ? g.LongCount(x =>
                        x.UsuarioSolicitante.Id == codigoUsuario.Value)
                    : 0,

                MiDepartamento = departamentoUsuario.HasValue
                    ? g.LongCount(x =>
                        x.UsuarioSolicitante.IdArea.HasValue &&
                        x.UsuarioSolicitante.IdArea.Value == departamentoUsuario.Value)
                    : 0
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new SolicitudResumenPaginadoDto();
    }

    [SuppressMessage(
      "Major Code Smell",
      "S3776:Cognitive Complexity of methods should not be too high",
      Justification = "ObtenerPaginado")]
    public async Task<PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>> ObtenerPaginado(
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
        CancellationToken cancellationToken)
    {
        SolicitudResumenPaginadoDto resumen = new();

        var queryuniverso = AplicarFiltrosUniverso(
            _context.Set<Solicitud>().AsNoTracking(),
            codigoUsuario,
            areaUsuario,
            roLUsuario,
            departamentoUsuario,
            fechainicio,
            fechafin,
            gerencia,
            departamento,
            solicitante,
            tiposolicitud,
            subtiposolicitud,
            tipoOperacion,
            aprobadorActual,
            estado,
            codigosolicitud,
            asunto,
            referencia,
            urgente,
            usuario,
            usuariodepartamento);

        if (incluirresumen == true)
        {
            resumen = await ObtenerResumenAsync(
            queryuniverso,
            codigoUsuario,
            departamentoUsuario,
            cancellationToken);
        }

        var querygrilla = AplicarFiltrosGrilla(
            queryuniverso,
            codigoUsuario,
            areaUsuario,
            roLUsuario,
            departamentoUsuario,
            urgente,
            usuario,
            usuariodepartamento);

        var projectedQuery = querygrilla
           .OrderByDescending(s => s.EsUrgente)
           .ThenBy(s => s.FechaSolicitud)
           .Select(s => new SolicitudPaginadoDto
           {
               IdSolicitud = s.Id,
               CodigoSolicitud = s.CodigoSolicitud,
               TipoSolicitud = s.TipoSolicitud.NombreTipoSolicitud,
               SubTipoSolicitud = s.SubTipoSolicitud.NombreSubTipoSolicitud,
               TipoOperacion = s.TipoOperacion.NombreTipoOperacion,
               Urgente = s.EsUrgente ? "Sí" : "No",
               Solicitante = (
                    (s.UsuarioSolicitante.Nombres.Contains(" ")
                        ? s.UsuarioSolicitante.Nombres.Substring(0, s.UsuarioSolicitante.Nombres.IndexOf(" "))
                        : s.UsuarioSolicitante.Nombres)
                    + " " +
                    (s.UsuarioSolicitante.ApellidoPaterno ?? "")
                ).Trim(),
               Estado = s.EstadoSolicitud.Nombre,
               FechaCreacion = s.FechaSolicitud,
               AprobadoresPendientes = s.SolicitudPlantilla.FirmantesFlujo
                            .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado && !sf.RevAprobado && sf.FechaRevision == null)
                            .Select(sf => sf.Id)
                            .Distinct()
                            .LongCount(),
               AprobadorActual = s.IdEstadoSolicitud == 4
                        ? "-"
                        : s.Documentos.Any()
                            ? s.Documentos
                                .SelectMany(d => d.DetallesFirma.Where(dd => dd.Estado))
                                .Where(df => df.SolicitudFirmanteFlujo.FirmanteFlujo.Estado && !df.SolicitudFirmanteFlujo.RevAprobado && df.SolicitudFirmanteFlujo.FechaRevision == null)
                                .OrderBy(df => df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
                                .Select(df => new
                                {
                                    df.IdSolicitudFirmanteFlujo,
                                    NombreCompleto = (
                                        (df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                                            ? df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                                                0, df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                                            : df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                                        + " " +
                                        (df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                                    ).Trim()
                                })
                                .Distinct()
                                .Select(x => x.NombreCompleto)
                                .FirstOrDefault()
                            : s.SolicitudPlantilla.FirmantesFlujo
                                .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado && !sf.RevAprobado && sf.FechaRevision == null)
                                .OrderBy(sf => sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
                                .Select(sf => (
                                    (sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                                        ? sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                                            0, sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                                        : sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                                    + " " +
                                    (sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                                ).Trim())
                                .FirstOrDefault()
           });

        if (paginationParams.Sorts?.Count > 0)
        {
            projectedQuery = paginationParams.Sorts.Count == 1
                ? projectedQuery.OrderByDynamic(paginationParams.Sorts[0].PropertyName, paginationParams.Sorts[0].Descending)
                : projectedQuery.OrderByMultiple(paginationParams.Sorts);
        }

        var totalCount = await projectedQuery.CountAsync(cancellationToken);

        var data = await projectedQuery
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);


        var pagedItems = new PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>(
          data,
          totalCount,
          paginationParams.PageNumber,
          paginationParams.PageSize)
        {
            ExtraData = resumen
        };

        return pagedItems;
    }

    #endregion

    #region BANDEJA ENTRADA

    [SuppressMessage(
  "Major Code Smell",
  "S3776:Cognitive Complexity of methods should not be too high",
  Justification = "Consulta compleja de para obtener solicitud para firma. Se mantiene temporalmente por compatibilidad con EF Core.")]
    public async Task<PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>> ObtenerBandejaEntrada(
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
       CancellationToken cancellationToken)
    {
        IQueryable<DetalleFirma> detalleFirmasQuery = _context.DetalleFirmas;

        IQueryable<SolicitudFirmanteFlujo> solicitudFirmanteFlujoQuery = _context.Set<SolicitudFirmanteFlujo>();

        BandejaEntradaResumenDto ResumenConDocumentos = new();
        BandejaEntradaResumenDto ResumenSinDocumentos = new();

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

        IQueryable<DetalleFirma> queryConDocumentos = detalleFirmasQuery
            .Where(df =>
                df.Estado &&
                codigoUsuario.HasValue &&
                df.SolicitudDocumento.Solicitud.IdEstadoSolicitud == 2 &&
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
                codigoUsuario.HasValue &&
                sf.SolicitudPlantilla.Solicitud.IdEstadoSolicitud == 2 &&
                sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value &&
                !sf.SolicitudPlantilla.Solicitud.Documentos.Any() &&
                !sf.RevAprobado &&
                sf.FechaRevision == null &&
                etapasPendientesMinimas.Any(e =>
                    e.IdSolicitud == sf.SolicitudPlantilla.Solicitud.Id &&
                    e.OrdenEtapaMinima == sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa
                ) &&
                (
                    (
                        sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == 1
                    )
                    ||
                    solicitudFirmanteFlujoQuery.Any(sfAnt =>
                     sfAnt.FirmanteFlujo.Estado &&
                        sfAnt.SolicitudPlantilla.Solicitud.Id == sf.SolicitudPlantilla.Solicitud.Id &&
                        sfAnt.FirmanteFlujo.PlantillaEtapa.Plantilla.Id == sf.FirmanteFlujo.PlantillaEtapa.Plantilla.Id &&
                        sfAnt.FirmanteFlujo.PlantillaEtapa.OrdenEtapa < sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa &&
                        sfAnt.RevAprobado &&
                        sfAnt.FechaRevision != null
                    )
                )
            );

        #region 1. Filtro Principales
        if (!string.IsNullOrWhiteSpace(fechainicio) &&
            DateTime.TryParse(fechainicio, out var fechaInicioValue))
        {
            var fechaInicioDate = fechaInicioValue.Date;

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.FechaCreacion >= fechaInicioDate);

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.FechaCreacion >= fechaInicioDate);
        }

        if (!string.IsNullOrWhiteSpace(fechafin) &&
            DateTime.TryParse(fechafin, out var fechaFinValue))
        {
            var fechaFinDate = fechaFinValue.Date.AddDays(1);

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.FechaCreacion < fechaFinDate);

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.FechaCreacion < fechaFinDate);
        }

        if (gerencia != null && gerencia.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
                gerencia.Contains(x.SolicitudDocumento.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.Value));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
                gerencia.Contains(x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.Value));
        }

        if (departamento != null && departamento.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.UsuarioSolicitante.IdArea.HasValue &&
                departamento.Contains(x.SolicitudDocumento.Solicitud.UsuarioSolicitante.IdArea.Value));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.IdArea.HasValue &&
                departamento.Contains(x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.IdArea.Value));
        }

        if (solicitante != null && solicitante.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                solicitante.Contains(x.SolicitudDocumento.Solicitud.IdUsuarioSolicitante));

            querySinDocumentos = querySinDocumentos.Where(x =>
                solicitante.Contains(x.SolicitudPlantilla.Solicitud.IdUsuarioSolicitante));
        }

        if (tiposolicitud != null && tiposolicitud.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                tiposolicitud.Contains(x.SolicitudDocumento.Solicitud.IdTipoSolicitud));

            querySinDocumentos = querySinDocumentos.Where(x =>
                tiposolicitud.Contains(x.SolicitudPlantilla.Solicitud.IdTipoSolicitud));
        }

        if (subtiposolicitud != null && subtiposolicitud.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                subtiposolicitud.Contains(x.SolicitudDocumento.Solicitud.IdSubTipoSolicitud));

            querySinDocumentos = querySinDocumentos.Where(x =>
                subtiposolicitud.Contains(x.SolicitudPlantilla.Solicitud.IdSubTipoSolicitud));
        }

        if (tipooperacion != null && tipooperacion.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                tipooperacion.Contains(x.SolicitudDocumento.Solicitud.IdTipoOperacion));

            querySinDocumentos = querySinDocumentos.Where(x =>
                tipooperacion.Contains(x.SolicitudPlantilla.Solicitud.IdTipoOperacion));
        }

        if (!string.IsNullOrWhiteSpace(codigosolicitud))
        {
            var codigoSolicitudFiltro = codigosolicitud.Trim();

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.CodigoSolicitud.Contains(codigoSolicitudFiltro));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.CodigoSolicitud.Contains(codigoSolicitudFiltro));
        }

        if (!string.IsNullOrWhiteSpace(asunto))
        {
            var asuntoFiltro = asunto.Trim();

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.Asunto != null &&
                x.SolicitudDocumento.Solicitud.Asunto.Contains(asuntoFiltro));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.Asunto != null &&
                x.SolicitudPlantilla.Solicitud.Asunto.Contains(asuntoFiltro));
        }

        if (!string.IsNullOrWhiteSpace(referencia))
        {
            var referenciaFiltro = referencia.Trim();

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.CodigosReferencia
                    .Any(r => r.CodigoReferencia.Valor.Contains(referenciaFiltro)));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.CodigosReferencia
                    .Any(r => r.CodigoReferencia.Valor.Contains(referenciaFiltro)));
        }
        #endregion

        var conDocumentos = await queryConDocumentos
            .Select(df => new
            {
                IdSolicitud = df.SolicitudDocumento.Solicitud.Id,
                df.SolicitudDocumento.Solicitud.CodigoSolicitud,
                TipoSolicitud = df.SolicitudDocumento.Solicitud.TipoSolicitud.NombreTipoSolicitud,
                SubTipoSolicitud = df.SolicitudDocumento.Solicitud.SubTipoSolicitud.NombreSubTipoSolicitud,
                TipoOperacion = df.SolicitudDocumento.Solicitud.TipoOperacion.NombreTipoOperacion,
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
                df.SolicitudDocumento.Solicitud.FechaCreacion,
                FechaLimite = df.SolicitudDocumento.Solicitud.FechaLimite != null
                    ? df.SolicitudDocumento.Solicitud.FechaLimite.Valor
                    : (DateTime?)null,
                IdPlantilla = df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var sinDocumentos = await querySinDocumentos
            .Select(sf => new
            {
                IdSolicitud = sf.SolicitudPlantilla.Solicitud.Id,
                sf.SolicitudPlantilla.Solicitud.CodigoSolicitud,
                TipoSolicitud = sf.SolicitudPlantilla.Solicitud.TipoSolicitud.NombreTipoSolicitud,
                SubTipoSolicitud = sf.SolicitudPlantilla.Solicitud.SubTipoSolicitud.NombreSubTipoSolicitud,
                TipoOperacion = sf.SolicitudPlantilla.Solicitud.TipoOperacion.NombreTipoOperacion,
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
                sf.SolicitudPlantilla.Solicitud.FechaCreacion,
                FechaLimite = sf.SolicitudPlantilla.Solicitud.FechaLimite != null
                    ? sf.SolicitudPlantilla.Solicitud.FechaLimite.Valor
                    : (DateTime?)null,
                IdPlantilla = sf.FirmanteFlujo.PlantillaEtapa.Plantilla.Id
            })
            .Distinct()
            .ToListAsync(cancellationToken);


        #region 2. Generar Resumen
        if (incluirresumen.HasValue && incluirresumen.Value)
        {
            ResumenConDocumentos = conDocumentos
                                        .GroupBy(_ => 1)
                                        .Select(g => new BandejaEntradaResumenDto
                                        {
                                            Total = g.LongCount(),
                                            Urgente = g.LongCount(x => x.EsUrgente == true)
                                        })
                                        .FirstOrDefault()
                                                ?? new BandejaEntradaResumenDto();

            ResumenSinDocumentos = sinDocumentos
                                        .GroupBy(_ => 1)
                                        .Select(g => new BandejaEntradaResumenDto
                                        {
                                            Total = g.LongCount(),
                                            Urgente = g.LongCount(x => x.EsUrgente == true)
                                        })
                                        .FirstOrDefault()
                                            ?? new BandejaEntradaResumenDto();
        }
        #endregion


        #region 3. Filtro Tabs(grilla)
        if (urgente.HasValue && urgente.Value)
        {
            conDocumentos = conDocumentos.Where(x => x.EsUrgente == urgente.Value).ToList();

            sinDocumentos = sinDocumentos.Where(x => x.EsUrgente == urgente.Value).ToList();
        }

        #endregion

        var baseResultado = conDocumentos
            .Concat(sinDocumentos)
            .GroupBy(x => new
            {
                x.IdSolicitud,
                x.CodigoSolicitud,
                x.TipoSolicitud,
                x.SubTipoSolicitud,
                x.TipoOperacion,
                x.EsUrgente,
                x.Solicitante,
                x.FechaCreacion,
                x.FechaLimite,
                x.IdPlantilla
            })
            .Select(g => g.First())
            .OrderByDescending(x => x.EsUrgente ? 1 : 0)
            .ThenBy(x => x.EsUrgente ? (x.FechaLimite ?? DateTime.MaxValue) : DateTime.MaxValue)
            .ThenBy(x => !x.EsUrgente ? x.FechaCreacion : DateTime.MaxValue)
            .ToList();

        //if (baseResultado.Count == 0)
        //    return [];

        var idsSolicitud = baseResultado
            .Select(x => x.IdSolicitud)
            .Distinct()
            .ToList();

        var plantillasIds = baseResultado
            .Select(x => x.IdPlantilla)
            .Distinct()
            .ToList();

        var firmantesRelacionados = await solicitudFirmanteFlujoQuery
            .Where(x =>
                x.Estado &&
                idsSolicitud.Contains(x.SolicitudPlantilla.Solicitud.Id) &&
                plantillasIds.Contains(x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id))
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

        var resultado = baseResultado
            .Select(x =>
            {
                var firmantesDeSolicitud = firmantesRelacionados
                    .Where(f => f.IdSolicitud == x.IdSolicitud && f.IdPlantilla == x.IdPlantilla)
                    .ToList();

                var ultimoFirmanteAprobado = firmantesDeSolicitud
                    .Where(f => f.RevAprobado && f.FechaRevision != null)
                    .OrderByDescending(f => f.FechaRevision)
                    .ThenByDescending(f => f.OrdenEtapa)
                    .Select(f => f.Firmante)
                    .FirstOrDefault();

                long? ultimaEtapa = firmantesDeSolicitud.Count > 0
                    ? firmantesDeSolicitud.Max(f => f.OrdenEtapa)
                    : null;

                return new BandejaEntradaDto
                {
                    IdSolicitud = x.IdSolicitud,
                    CodigoSolicitud = x.CodigoSolicitud,
                    TipoSolicitud = x.TipoSolicitud,
                    SubTipoSolicitud = x.SubTipoSolicitud,
                    TipoOperacion = x.TipoOperacion,
                    Urgente = x.EsUrgente ? "Si" : "No",
                    Solicitante = x.Solicitante,
                    FechaCreacion = x.FechaCreacion,
                    FechaLimite = x.FechaLimite,

                    AnteriorFirmante = ultimoFirmanteAprobado ?? string.Empty,

                    UltimoFirmante = ultimaEtapa.HasValue
                        ? firmantesDeSolicitud
                            .Where(f => f.OrdenEtapa == ultimaEtapa.Value)
                            .Select(f => f.Firmante)
                            .Distinct()
                            .ToList()
                        : []
                };
            }).ToList();

        var totalCount = resultado.Count;

        var data = resultado
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();


        var pagedItems = new PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>(
          data,
          totalCount,
          paginationParams.PageNumber,
          paginationParams.PageSize)
        ;

        pagedItems.ExtraData = new BandejaEntradaResumenDto
        {
            Total = ResumenConDocumentos.Total + ResumenSinDocumentos.Total,
            Urgente = ResumenConDocumentos.Urgente + ResumenSinDocumentos.Urgente
        };

        return pagedItems;
    }

    #endregion

    #region SOLICITUD SUSTENTO

    public async Task<SolicitudSustento?> ObtenerSolicitudSustentoPorId(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        return await _context.SolicitudSustentos
             .AsNoTracking()
             .FirstOrDefaultAsync(s => s.Id == id);
    }

    #endregion

    #region SOLICITUD DOCUMENTO

    public async Task<SolicitudDocumento?> ObtenerSolicitudDocumentoPorId(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        var query = _context.SolicitudDocumentos
            .Include(sd => sd.Solicitud)
            .Include(sd => sd.DetallesFirma.Where(df => df.Estado))
            .Where(sd => sd.Id == id);

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<IReadOnlyCollection<ArchivoDescargaDto>> ObtenerDocumentosFirmadosPorSolicitudAsync(
           long idSolicitud,
           CancellationToken cancellationToken,
           bool tracking = false)
    {
        var query = _context.DetalleFirmas
            .Where(df =>
                df.SolicitudDocumento.IdSolicitud == idSolicitud &&
                df.FechaHoraFirma != null &&
                df.NombreDocumentoFirmado != null &&
                df.RutaDocumentoFirmado != null);

        if (!tracking)
            query = query.AsNoTracking();

        var resultado = await query
            .GroupBy(df => df.IdSolicitudDocumento)
            .Select(g => g
                .OrderByDescending(x => x.FechaHoraFirma)
                .Select(x => new ArchivoDescargaDto(
                    x.NombreDocumentoFirmado! + "." + x.ExtensionDocumentoFirmado,
                    x.RutaDocumentoFirmado! + "\\" +
                    x.NombreDocumentoFirmado + "." +
                    x.ExtensionDocumentoFirmado))
                .First())
            .ToListAsync(cancellationToken);

        return resultado;
    }

    #endregion
}
