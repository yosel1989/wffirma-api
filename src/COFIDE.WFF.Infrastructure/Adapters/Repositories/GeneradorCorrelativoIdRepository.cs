using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class GeneradorCorrelativoIdRepository(
        WorkflowFirmaDbContext context
    ) : IGeneradorCorrelativoIdRepository
{
    private readonly WorkflowFirmaDbContext _context = context;

    #region PLANTILLA - ETAPA - FIRMANTES
    public async Task<long> ObtenerSiguienteIdPlantillaAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Plantillas
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdEtapaAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.PlantillaEtapas
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdFirmanteAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.FirmanteFlujos
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }


    public async Task<List<long>> ObtenerBloqueSiguienteIdEtapaAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.PlantillaEtapas
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

    public async Task<List<long>> ObtenerBloqueSiguienteIdFirmanteAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.FirmanteFlujos
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
    #endregion

    #region SOLICITUD

    public async Task<long> ObtenerSiguienteIdSolicitudAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<Solicitud>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdMovimientoSolicitudAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<MovimientoSolicitud>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdComentarioAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<SolicitudComentario>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }
    public async Task<long> ObtenerSiguienteIdSolicitudPlantillaAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.SolicitudPlantillas
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdSolitanteFirmanteAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<SolicitudFirmanteFlujo>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(long cantidad, CancellationToken cancellationToken)
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

    public async Task<List<long>> ObtenerBloqueSiguienteIdDetalleFirmaAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.DetalleFirmas
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

    public async Task<List<long>> ObtenerBloqueSiguienteIdCodigosReferenciaAsync(long cantidad, CancellationToken cancellationToken)
    {
        if (cantidad <= 0)
            return [];

        var maxId = await _context.Set<SolicitudCodReferencia>()
          .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>((int)cantidad);

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdCorreosAsync(long cantidad, CancellationToken cancellationToken)
    {
        if (cantidad <= 0)
            return [];

        var maxId = await _context.Set<SolicitudCorreo>()
          .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>((int)cantidad);

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdDocumentoAsync(long cantidad, CancellationToken cancellationToken)
    {
        if (cantidad <= 0)
            return [];

        var maxId = await _context.Set<SolicitudDocumento>()
          .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>((int)cantidad);

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdSustentoAsync(long cantidad, CancellationToken cancellationToken)
    {
        if (cantidad <= 0)
            return [];

        var maxId = await _context.Set<SolicitudSustento>()
          .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>((int)cantidad);

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdDocumentoTagAsync(long cantidad, CancellationToken cancellationToken)
    {
        if (cantidad <= 0)
            return [];

        var maxId = await _context.Set<DocumentoTag>()
          .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>((int)cantidad);

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<long> ObtenerSiguienteIdHistorialRevisionAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Set<HistorialRevision>()
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }


    #endregion

    #region CODIGO SOLICITUD
    public async Task<long> ObtenerSiguienteCodigoSolicitudCorrelativoAsync(
    string codArea,
    string codTipoSolicitud,
    int year,
    CancellationToken cancellationToken)
    {
        var prefijo = $"{codArea}-{codTipoSolicitud}-";
        var sufijo = $"-{year}";

        var codigos = await _context.Solicitudes
            .AsNoTracking()
            .Where(s => s.CodigoSolicitud.StartsWith(prefijo) &&
                        s.CodigoSolicitud.EndsWith(sufijo))
            .Select(s => s.CodigoSolicitud)
            .ToListAsync(cancellationToken);

        long max = 0;

        foreach (var codigo in codigos)
        {
            var partes = codigo.Split('-');

            // Validación estricta de formato esperado: AREA-TIPO-CORRELATIVO-AÑO
            if (partes.Length != 4)
                continue;

            if (!long.TryParse(partes[2], out var correlativo))
                continue;

            if (correlativo > max)
                max = correlativo;
        }

        return max + 1;
    }

    #endregion


   
}
