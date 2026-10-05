using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class SolicitudDocumentoRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<SolicitudDocumento, long>(context), ISolicitudDocumentoRepository
{

    [SuppressMessage(
     "Major Code Smell",
     "S3776:Cognitive Complexity of methods should not be too high",
     Justification = "ObtenerGestionDocumentosBloqueadosPaginado")]
    public async Task<PagedResult<DocumentoBloqueadoPaginadoDto>> ObtenerGestionDocumentosBloqueadosPaginado(
        PagedQueryParams paginationParams,
        string? fechaInicio,
        string? fechaFin,
        string? nombreArchivo,
        string? codigoSolicitud,
        List<long>? firmanteActual,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<SolicitudDocumento>()
            .AsNoTracking()
            .Where(d => d.Estado)
            .SelectMany(d => d.DetallesFirma
                .Where(df => df.Estado && df.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado)
                .Select(df => new
                {
                    IdDocumento = d.Id,
                    d.IdSolicitud,
                    NombreArchivo = d.NombreOriginal + "." + d.ExtensionOriginal,
                    df.IdSolicitudFirmanteFlujo,
                    FechaBloqueo = df.FechaInicioProceso,
                    Estado = df.EstadoDocumento.Nombre
                }));

        if (!string.IsNullOrWhiteSpace(nombreArchivo))
        {
            var filtro = nombreArchivo.Trim();
            query = query.Where(x => x.NombreArchivo.Contains(filtro));
        }

        if (!string.IsNullOrWhiteSpace(codigoSolicitud))
        {
            var filtro = codigoSolicitud.Trim();
            query = query.Where(x =>
                _context.Set<Solicitud>().Any(s =>
                    s.Id == x.IdSolicitud &&
                    s.CodigoSolicitud.Contains(filtro)));
        }

        if (firmanteActual is not null && firmanteActual.Count > 0)
        {
            query = query.Where(x =>
                _context.Set<SolicitudFirmanteFlujo>().Any(sf =>
                    sf.Id == x.IdSolicitudFirmanteFlujo &&
                    _context.Set<FirmanteFlujo>().Any(ff =>
                        ff.Id == sf.IdFirmanteFlujo &&
                        firmanteActual.Contains(ff.IdUsuarioAsignado))));
        }

        if (!string.IsNullOrWhiteSpace(fechaInicio) && DateTime.TryParse(fechaInicio, CultureInfo.InvariantCulture,
        DateTimeStyles.RoundtripKind, out var fi))
        {
            var inicio = fi.Date;
            query = query.Where(x => x.FechaBloqueo.HasValue && x.FechaBloqueo.Value >= inicio);
        }

        if (!string.IsNullOrWhiteSpace(fechaFin) && DateTime.TryParse(fechaFin, CultureInfo.InvariantCulture,
        DateTimeStyles.RoundtripKind, out var ff))
        {
            var fin = ff.Date.AddDays(1);
            query = query.Where(x => x.FechaBloqueo.HasValue && x.FechaBloqueo.Value < fin);
        }

        var pagina = await query
            .OrderByDescending(x => x.FechaBloqueo)
            .ToListAsync(cancellationToken);

        var idsSolicitud = pagina.Select(x => x.IdSolicitud).Distinct().ToList();
        var idsDoc = pagina.Select(x => x.IdDocumento).Distinct().ToList();
        var idsFlujo = pagina.Select(x => x.IdSolicitudFirmanteFlujo).Distinct().ToList();

        var codigos = await _context.Set<Solicitud>()
            .AsNoTracking()
            .Where(x => idsSolicitud.Contains(x.Id))
            .Select(x => new { x.Id, x.CodigoSolicitud })
            .ToDictionaryAsync(x => x.Id, x => x.CodigoSolicitud, cancellationToken);

        var (firmanteActualMap, firmanteAnteriorPorDocumento) =
            await ResolverNombresFirmantesAsync(idsFlujo, idsDoc, cancellationToken);


        var totalCount = pagina.Count;

        var data = pagina
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var items = data.Select(x => new DocumentoBloqueadoPaginadoDto
        {
            IdArchivo = x.IdDocumento.ToString(),
            NombreArchivo = x.NombreArchivo,
            CodigoSolicitud = codigos.TryGetValue(x.IdSolicitud, out var codigo)
                ? codigo : string.Empty,
            FirmanteActual = firmanteActualMap.TryGetValue(x.IdSolicitudFirmanteFlujo, out var actual)
                ? actual : string.Empty,
            FirmanteAnterior = firmanteAnteriorPorDocumento.TryGetValue(x.IdDocumento, out var anterior)
                ? anterior : string.Empty,
            FechaBloqueo = x.FechaBloqueo,
            Estado = x.Estado
        }).ToList();

        return new PagedResult<DocumentoBloqueadoPaginadoDto>(items, totalCount);
    }

    private async Task<
(
    Dictionary<long, string> firmanteActualMap,
    Dictionary<long, string> firmanteAnteriorPorDocumento
)>
ResolverNombresFirmantesAsync(
    List<long> idsFlujo,
    List<long> idsDocumento,
    CancellationToken cancellationToken)
    {
        var datos = await _context.Set<DetalleFirma>()
            .AsNoTracking()
            .Where(df =>
                df.Estado &&
                idsDocumento.Contains(df.SolicitudDocumento.Id))
            .Select(df => new
            {
                IdDocumento = df.SolicitudDocumento.Id,
                df.IdSolicitudFirmanteFlujo,

                df.SolicitudFirmanteFlujo
                    .FirmanteFlujo
                    .PlantillaEtapa
                    .OrdenEtapa,

                df.SolicitudFirmanteFlujo.RevAprobado,
                df.SolicitudFirmanteFlujo.FechaRevision,

                Firmante = (
                    (
                        df.SolicitudFirmanteFlujo
                            .FirmanteFlujo
                            .FirmanteFlujoUsuarioAsignado
                            .Nombres.Contains(' ')
                        ?
                        df.SolicitudFirmanteFlujo
                            .FirmanteFlujo
                            .FirmanteFlujoUsuarioAsignado
                            .Nombres.Substring(
                                0,
                                df.SolicitudFirmanteFlujo
                                    .FirmanteFlujo
                                    .FirmanteFlujoUsuarioAsignado
                                    .Nombres.IndexOf(' '))
                        :
                        df.SolicitudFirmanteFlujo
                            .FirmanteFlujo
                            .FirmanteFlujoUsuarioAsignado
                            .Nombres
                    )
                    + " " +
                    (
                        df.SolicitudFirmanteFlujo
                            .FirmanteFlujo
                            .FirmanteFlujoUsuarioAsignado
                            .ApellidoPaterno ?? ""
                    )
                ).Trim()
            })
            .ToListAsync(cancellationToken);

        // Firmante actual
        var firmanteActualMap = datos
            .Where(x => idsFlujo.Contains(x.IdSolicitudFirmanteFlujo))
            .GroupBy(x => x.IdSolicitudFirmanteFlujo)
            .ToDictionary(
                g => g.Key,
                g => g.First().Firmante);

        // Firmante anterior (último aprobado)
        var firmanteAnteriorPorDocumento = datos
            .GroupBy(x => x.IdDocumento)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Where(x =>
                        x.RevAprobado &&
                        x.FechaRevision != null)
                    .OrderByDescending(x => x.FechaRevision)
                    .ThenByDescending(x => x.OrdenEtapa)
                    .Select(x => x.Firmante)
                    .FirstOrDefault() ?? string.Empty);

        return (firmanteActualMap, firmanteAnteriorPorDocumento);
    }

    public async Task<int> ContarGestionDocumentosBloqueados(
        string? fechaInicio,
        string? fechaFin,
        string? nombreArchivo,
        string? codigoSolicitud,
        List<long>? firmanteActual,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<SolicitudDocumento>()
            .AsNoTracking()
            .Where(d => d.Estado)
            .SelectMany(d => d.DetallesFirma
                .Where(df =>
                    df.Estado &&
                    df.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado)
                .Select(df => new
                {
                    IdDocumento = d.Id,
                    d.IdSolicitud,
                    NombreArchivo = d.NombreOriginal + "." + d.ExtensionOriginal,
                    df.IdSolicitudFirmanteFlujo,
                    df.FechaInicioProceso
                }));

        if (!string.IsNullOrWhiteSpace(nombreArchivo))
        {
            var filtro = nombreArchivo.Trim();
            query = query.Where(x => x.NombreArchivo.Contains(filtro));
        }

        if (!string.IsNullOrWhiteSpace(codigoSolicitud))
        {
            var filtro = codigoSolicitud.Trim();

            query = query.Where(x =>
                _context.Set<Solicitud>().Any(s =>
                    s.Id == x.IdSolicitud &&
                    s.CodigoSolicitud.Contains(filtro)));
        }

        if (firmanteActual is not null && firmanteActual.Count > 0)
        {
            query = query.Where(x =>
                _context.Set<SolicitudFirmanteFlujo>().Any(sf =>
                    sf.Id == x.IdSolicitudFirmanteFlujo &&
                    _context.Set<FirmanteFlujo>().Any(ff =>
                        ff.Id == sf.IdFirmanteFlujo &&
                        firmanteActual.Contains(ff.IdUsuarioAsignado))));
        }

        if (!string.IsNullOrWhiteSpace(fechaInicio) &&
            DateTime.TryParse(fechaInicio, CultureInfo.InvariantCulture,
        DateTimeStyles.RoundtripKind, out var fechaInicioValue))
        {
            var inicio = fechaInicioValue.Date;

            query = query.Where(x =>
                x.FechaInicioProceso.HasValue &&
                x.FechaInicioProceso.Value >= inicio);
        }

        if (!string.IsNullOrWhiteSpace(fechaFin) &&
            DateTime.TryParse(fechaFin, CultureInfo.InvariantCulture,
        DateTimeStyles.RoundtripKind, out var fechaFinValue))
        {
            var fin = fechaFinValue.Date.AddDays(1);

            query = query.Where(x =>
                x.FechaInicioProceso.HasValue &&
                x.FechaInicioProceso.Value < fin);
        }

        return await query
            .Select(x => x.IdDocumento)
            .Distinct()
            .CountAsync(cancellationToken);
    }
    public async Task<SolicitudDocumento?> ObtenerSolicitudDocumentoParaFirmadoInterno(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        var query = _context.SolicitudDocumentos
           .Include(sd => sd.Solicitud)
                .ThenInclude(sd => sd.SolicitudPlantilla)
                    .ThenInclude(sd => sd.FirmantesFlujo)
           .Include(sd => sd.DetallesFirma.Where(sdd => sdd.Estado))
                .ThenInclude(sd => sd.UsuarioFirmante)
           .Where(sd => sd.Id == id);

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }


    #region DETALLE FIRMA

    public async Task<DetalleFirma?> ObtenerDetalleFirmaPorDocumentoAsignado(long idSolicitudDocumento, long idUsuarioAsignado, CancellationToken cancellationToken)
    {
        return await _context.DetalleFirmas
         .FirstOrDefaultAsync(x => x.IdSolicitudDocumento == idSolicitudDocumento && x.Estado
         && x.IdFirmanteDocumento == idUsuarioAsignado, cancellationToken);
    }

    #endregion
}
