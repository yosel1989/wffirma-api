using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using ValueObject = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class DetalleFirmaRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<DetalleFirma, long>(context), IDetalleFirmaRepository
{



    public async Task<List<DetalleFirma>> ObtenerDetalleFirmaPorCodigoFirma(
      string codigoFirma,
      CancellationToken cancellationToken)
    {
        var idsDocumento = await _context.DetalleFirmas
             .Where(x => x.CodigoFirma == codigoFirma && x.Estado)
             .Select(x => x.IdSolicitudDocumento)
             .Distinct()
             .ToListAsync(cancellationToken);

        return await _context.DetalleFirmas
            .Include(x => x.SolicitudDocumento)
                .ThenInclude(x => x.Solicitud)
            .Include(x => x.SolicitudDocumento)
                .ThenInclude(x => x.DocumentoTags)

            .Where(x => x.Estado && idsDocumento.Contains(x.IdSolicitudDocumento))
            .ToListAsync(cancellationToken);
    }

    public async Task<DetalleFirma?> ValidarDocumentoFirmadoAsync(
    long idDetalleFirma,
    CancellationToken cancellationToken)
    {
        var idSolicitudDocumento = await _context.DetalleFirmas
            .AsNoTracking()
            .Where(x => x.Id == idDetalleFirma && x.Estado)
            .Select(x => x.IdSolicitudDocumento)
            .FirstOrDefaultAsync(cancellationToken);

        if (idSolicitudDocumento == 0) return null;

        return await _context.DetalleFirmas
            .AsNoTracking()
            .Include(x => x.UsuarioFirmante)
            .Where(x =>
                x.IdSolicitudDocumento == idSolicitudDocumento &&
                x.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado
                && x.Estado)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<DetalleFirma>> ValidarDocumentoFirmadoAsync(
    List<long> idsDetalleFirma,
    CancellationToken cancellationToken)
    {
        var idsSolicitudDocumento = await _context.DetalleFirmas
            .AsNoTracking()
            .Where(x => idsDetalleFirma.Contains(x.Id) && x.Estado)
            .Select(x => x.IdSolicitudDocumento)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (idsSolicitudDocumento.Count == 0)
            return [];

        return await _context.DetalleFirmas
            .AsNoTracking()
            .Include(x => x.UsuarioFirmante)
            .Where(x =>
                idsSolicitudDocumento.Contains(x.IdSolicitudDocumento) &&
                x.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado &&
                x.Estado)
            .ToListAsync(cancellationToken);
    }

    public async Task<DetalleFirma?> ObtenerDetalleBloqueadoPorDocumentoAsync(
    long idDocumento,
    CancellationToken cancellationToken)
    {
        return await _context.Set<DetalleFirma>()
            .Include(x => x.SolicitudDocumento)
            .FirstOrDefaultAsync(x =>
                x.Estado &&
                x.IdSolicitudDocumento == idDocumento &&
                x.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado,
                cancellationToken);
    }

    public async Task<List<DetalleFirma>> ObtenerDocumentosEnEstadoFirmadoAsync(
    List<long> idsDetalleFirma,
    CancellationToken cancellationToken)
    {
        return await _context.DetalleFirmas
            .AsNoTracking()
            .Where(x =>
                x.EstadoDocumento == ValueObject.EstadoDocumento.Firmado &&
                idsDetalleFirma.Contains(x.Id) && x.Estado)
            .ToListAsync(cancellationToken);
    }


    #region JOB

    public async Task<int> ActualizarEstadosSolicitudProgramadoAsync(CancellationToken cancellationToken)
    {
        var ahora = DateTime.Now;

        var registros = await _context.Set<DetalleFirma>()
            .Where(x => x.EstadoDocumento == ValueObject.EstadoDocumento.Bloqueado && x.FechaFinEsperado <= ahora)
            .ToListAsync(cancellationToken);

        foreach (var item in registros)
        {
            item.ActivarDocumento();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return registros.Count;
    }

    #endregion
}
