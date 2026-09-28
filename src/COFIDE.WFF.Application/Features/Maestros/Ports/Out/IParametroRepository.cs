using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Jobs.DTOs;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Maestros.Ports.Out;
public interface IParametroRepository
{
    Task<List<ParametroDetalle>> ObtenerParametrosAsync(CancellationToken cancellationToken, bool tracking = false);
    Task<ParametroDetalle?> ObtenerParametroDetallePorIdAsync(long id, CancellationToken cancellationToken);
    Task<ParametroDetalleDto?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);
    Task<List<ParametroDetalleDto>> ObtenerDetallePorCodigoParametro(string codigoParametro, CancellationToken cancellationToken);
    Task<List<ParametroDetalleDto>> ObtenerCodigoParametroDias(string codigoDetalle, CancellationToken cancellationToken);
    Task<ConfiguracionJobDto?> ObtenerConfiguracionAsync(CancellationToken cancellationToken);
    Task<TiempoProcesoFirmaDto> ObtenerTiempoProcesoFirmadoArchivoAsync(CancellationToken cancellationToken);
    Task<TamanioFirmaDto> ObtenerTamanioFirmaAsync(CancellationToken cancellationToken);
    Task<PosicionFirmaDto> ObtenerPosicionFirmaAsync(CancellationToken cancellationToken);
    Task<ConfiguracionFirmaDto> ConfiguracionFirmaAsync(CancellationToken cancellationToken);
}
