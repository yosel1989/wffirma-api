using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Jobs.DTOs;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class ParametroRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<ParametroDetalle, long>(context), IParametroRepository
{
    public async Task<List<ParametroDetalle>> ObtenerParametrosAsync(CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<ParametroDetalle> query = _context.Set<ParametroDetalle>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(d => d.Cabecera)
            .Where(d => d.Estado && d.Cabecera.Estado);

        return await query
            .OrderBy(d => d.Cabecera.CodigoParametro)
            .ThenBy(d => d.CodigoDetalle)
            .ToListAsync(cancellationToken);
    }


    public async Task<ParametroDetalle?> ObtenerParametroDetallePorIdAsync(long id, CancellationToken cancellationToken)
    {
        IQueryable<ParametroDetalle> query = _context.Set<ParametroDetalle>();

        query = query.Where(d => d.Estado && d.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }


    // -- detalle

    public async Task<ParametroDetalleDto?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.Set<ParametroDetalle>()
         .Where(x => x.Id == id)
         .Select(x => new ParametroDetalleDto
         {
             Id = x.Id,
             Codigo = x.CodigoDetalle,
             Valor = x.ValorParametro
         })
         .FirstAsync(cancellationToken);
    }

    public async Task<List<ParametroDetalleDto>> ObtenerDetallePorCodigoParametro(string codigoParametro, CancellationToken cancellationToken)
    {
        return await _context.ParametroDetalles
            .Include(x => x.Cabecera)
            .Where(x => x.Cabecera.CodigoParametro == codigoParametro)
             .Select(x => new ParametroDetalleDto
             {
                 Id = x.Id,
                 Codigo = x.CodigoDetalle,
                 Valor = x.ValorParametro
             })
         .ToListAsync(cancellationToken);
    }

    public async Task<List<ParametroDetalleDto>> ObtenerCodigoParametroDias(string codigoDetalle, CancellationToken cancellationToken)
    {
        return await _context.ParametroDetalles
            .Where(x => x.CodigoDetalle == codigoDetalle)
            .Select(x => new ParametroDetalleDto
            {
                Id = x.Id,
                Codigo = x.CodigoDetalle,
                Valor = x.ValorParametro
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfiguracionJobDto?> ObtenerConfiguracionAsync(CancellationToken cancellationToken)
    {
        var parametros = await _context.Set<ParametroDetalle>()
    .Where(x =>
        x.CodigoDetalle == "HO_INICIO_JOB" ||
        x.CodigoDetalle == "NU_FRECUENCIA_JOB")
    .ToListAsync(cancellationToken);

        var hora = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "HO_INICIO_JOB")?.ValorParametro;

        var frecuencia = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "NU_FRECUENCIA_JOB")?.ValorParametro;

        if (hora == null || frecuencia == null)
            return null;

        return new ConfiguracionJobDto
        {
            HoraInicio = int.Parse(hora),
            FrecuenciaMinutos = int.Parse(frecuencia)
        };
    }

    public async Task<TiempoProcesoFirmaDto> ObtenerTiempoProcesoFirmadoArchivoAsync(CancellationToken cancellationToken)
    {
        List<string> codigos = new List<string> { "TI_ARCHIVO", "TI_MARGEN_SEGURIDAD" };

        var parametros = await _context.Set<ParametroDetalle>()
            .Where(x => codigos.Contains(x.CodigoDetalle))
            .ToListAsync(cancellationToken);

        var tiempoArchivo = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "TI_ARCHIVO")?.ValorParametro;

        var tiempoMargenSeguridad = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "TI_MARGEN_SEGURIDAD")?.ValorParametro;

        return new TiempoProcesoFirmaDto
        {
            TiempoArchivo = int.Parse(tiempoArchivo ?? "4"),
            TiempoMargenSeguridad = int.Parse(tiempoMargenSeguridad ?? "60")
        };
    }


    public async Task<TamanioFirmaDto> ObtenerTamanioFirmaAsync(CancellationToken cancellationToken)
    {
        List<string> codigos = new List<string> { "DM_FIRMA_ANCHO", "DM_FIRMA_ALTO", "DM_VISADO_ANCHO", "DM_VISADO_ALTO" };

        var parametros = await _context.Set<ParametroDetalle>()
            .Where(x => codigos.Contains(x.CodigoDetalle))
            .ToListAsync(cancellationToken);

        var firmaAltoFirmante = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "DM_FIRMA_ALTO")?.ValorParametro;

        var firmaAnchoFirmante = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "DM_FIRMA_ANCHO")?.ValorParametro;

        var firmaAltoVisador = parametros
           .FirstOrDefault(x => x.CodigoDetalle == "DM_VISADO_ALTO")?.ValorParametro;

        var firmaAnchoVisador = parametros
           .FirstOrDefault(x => x.CodigoDetalle == "DM_VISADO_ANCHO")?.ValorParametro;

        return new TamanioFirmaDto
        {
            FirmaAltoFirmante = decimal.Parse(firmaAltoFirmante ?? "45"),
            FirmaAnchoFirmante = decimal.Parse(firmaAnchoFirmante ?? "175"),
            FirmaAltoVisador = decimal.Parse(firmaAltoVisador ?? "90"),
            FirmaAnchoVisador = decimal.Parse(firmaAnchoVisador ?? "85")
        };
    }

    public async Task<PosicionFirmaDto> ObtenerPosicionFirmaAsync(CancellationToken cancellationToken)
    {
        List<string> codigos = new List<string> { "PS_VISADO_X", "PS_VISADO_Y" };

        var parametros = await _context.Set<ParametroDetalle>()
            .Where(x => codigos.Contains(x.CodigoDetalle))
            .ToListAsync(cancellationToken);

        var posicionXVisador = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "PS_VISADO_X")?.ValorParametro;

        var posicionYVisador = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "PS_VISADO_Y")?.ValorParametro;

        return new PosicionFirmaDto
        {
            PosicionXVisador = decimal.Parse(posicionXVisador ?? "0"),
            PosicionYVisador = decimal.Parse(posicionYVisador ?? "750")
        };
    }

    public async Task<ConfiguracionFirmaDto> ConfiguracionFirmaAsync(CancellationToken cancellationToken)
    {
        List<string> codigos = new List<string> { "CF_VISADO_TAMLETRA", "CF_FIRMA_TAMLETRA", 
            "CF_FIRMA_ETIQUETA", "CF_COLOR_ETIQUETA", "CF_FIRMA_MARGENALTO" };

        var parametros = await _context.Set<ParametroDetalle>()
            .Where(x => codigos.Contains(x.CodigoDetalle))
            .ToListAsync(cancellationToken);

        var tamanioLetraVisado = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "CF_VISADO_TAMLETRA")?.ValorParametro;

        var tamanioLetraFirmante = parametros
           .FirstOrDefault(x => x.CodigoDetalle == "CF_FIRMA_TAMLETRA")?.ValorParametro;

        var nombreTAG = parametros
            .FirstOrDefault(x => x.CodigoDetalle == "CF_FIRMA_ETIQUETA")?.ValorParametro;

        var colorEtiqueta = parametros
           .FirstOrDefault(x => x.CodigoDetalle == "CF_COLOR_ETIQUETA")?.ValorParametro;     

        var firmaMargenAlto = parametros
         .FirstOrDefault(x => x.CodigoDetalle == "CF_FIRMA_MARGENALTO")?.ValorParametro;

        return new ConfiguracionFirmaDto
        {
            TamanioLetraVisado = int.Parse(tamanioLetraVisado ?? "6"),
            TamanioLetraFirmante = int.Parse(tamanioLetraFirmante ?? "6"),
            NombreTag = string.IsNullOrWhiteSpace(nombreTAG) ? "[FIRMA]" : nombreTAG,
            ColorEtiqueta = string.IsNullOrWhiteSpace(colorEtiqueta) ? "#FFFFFF" : colorEtiqueta,
            FirmaMargenAlto = decimal.Parse(firmaMargenAlto ?? "30")
        };
    }


}
