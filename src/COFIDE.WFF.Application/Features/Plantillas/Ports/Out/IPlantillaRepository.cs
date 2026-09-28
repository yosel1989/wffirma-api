using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Common.Pageds;
namespace COFIDE.WFF.Application.Features.Plantillas.Ports.Out;

public interface IPlantillaRepository : IGenericRepository<Plantilla, long>
{
    Task<Plantilla?> ObtenerAgregadoPlantillaAsync(long id, CancellationToken cancellationToken, bool tracking = false);
    Task<long> ObtenerSiguienteIdAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdEtapaAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdFirmanteAsync(CancellationToken cancellationToken);
    void GuardarFlujoPlantillaAsync(Plantilla? plantilla);
    Task<List<long>> ObtenerBloqueSiguienteIdEtapaAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdFirmanteAsync(long cantidad, CancellationToken cancellationToken);
    Task<Plantilla?> ValidarExistenciaNombrePlantilla(string nombrePlantilla, bool visibilidad, long usuarioId);

    #region Queries

    Task<PlantillaDto?> ObtenerDetallePlantillaPorId(long IdPlantilla);
    Task<PagedResult<PlantillaPaginadoDto>> ObtenerPaginado(PlantillaPaginadoRequest requestPaginado);

    #endregion

    #region Etapas

    Task<List<PlantillaEtapa>> ObtenerPlantillaEtapaPorIdPlantilla(long idPlantilla, CancellationToken cancellationToken, bool tracking = false);

    #endregion
}