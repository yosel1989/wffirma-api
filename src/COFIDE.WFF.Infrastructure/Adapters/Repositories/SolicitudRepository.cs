using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
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


    #region Aprobacion

    public async Task<Solicitud?> ObtenerParaAprobacion(long id, CancellationToken cancellationToken)
    {
        return await _context.Solicitudes
            .Include(s => s.Documentos.Where(sd => sd.Estado))
            .ThenInclude(d => d.DetallesFirma.Where(df => df.Estado))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
    #endregion



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

    public async Task<Solicitud?> ObtenerSolicitudParaRestauracion(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<Solicitud> query = _context.Set<Solicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(s => s.SolicitudPlantilla)
                .ThenInclude(s => s.FirmantesFlujo)
                    .ThenInclude(f => f.HistRevision.Where(h => h.Estado))
            .Include(s => s.Documentos.Where(sd => sd.Estado))
                .ThenInclude(s => s.DetallesFirma);

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

                foreach (var historialRevision in firmanteFlujo.HistRevision)
                {
                    _context.Entry(historialRevision).State =
                    historialRevision.EsNuevo
                        ? EntityState.Added
                        : EntityState.Modified;
                }
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



    #endregion

    #region HISTORIAL REVISIONES
    public async Task<int> ObtenerSiguienteCicloAsync(long idSolicitudPlantilla, CancellationToken cancellationToken)
    {
        var maxCiclo = await _context.Set<HistorialRevision>()
        .Where(x => x.Estado && x.IdSolicitudPlantilla == idSolicitudPlantilla)
        .MaxAsync(x => (int?)x.Ciclo, cancellationToken) ?? 0;

        if (maxCiclo == 0)
            return 1;

        var aprobaciones = await _context.Set<HistorialRevision>()
            .AsNoTracking()
            .Where(x => x.Estado
                            && x.IdSolicitudPlantilla == idSolicitudPlantilla
                            && x.Ciclo == maxCiclo)
            .Select(x => x.RevAprobado)
            .ToListAsync(cancellationToken);

        var cicloCerrado = aprobaciones.Any(aprobado => !aprobado);

        return cicloCerrado ? maxCiclo + 1 : maxCiclo;
    }
    #endregion
}
