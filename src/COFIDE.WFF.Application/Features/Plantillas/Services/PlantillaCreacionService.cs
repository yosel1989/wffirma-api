using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Application.Common.Constants;
namespace COFIDE.WFF.Application.Features.Plantillas.Services;

public class PlantillaCreacionService(
        IPlantillaRepository plantillaRepository,
        IParametroRepository parametroRepository
    ) : IPlantillaCreacionService
{
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IParametroRepository _parametroRepository= parametroRepository;

    public async Task<Plantilla> CrearPlantilla(
            CrearPlantillaInput input,
            long usuarioActual,
            string gerenciaUsuario,
            CancellationToken cancellationToken)
    {
        var idPlantilla = await _plantillaRepository.ObtenerSiguienteIdAsync(cancellationToken);
        var idEtapa = await _plantillaRepository.ObtenerSiguienteIdEtapaAsync(cancellationToken);
        var idFirmante = await _plantillaRepository.ObtenerSiguienteIdFirmanteAsync(cancellationToken);

        var tipoEtapaDict = (await _parametroRepository
             .ObtenerDetallePorCodigoParametro(CodigosParametro.TipoEtapa, cancellationToken))
             .ToDictionary(
                 x => x.Id,
                 x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
             );

        var tipoAprobadorDict = (await _parametroRepository
             .ObtenerDetallePorCodigoParametro(CodigosParametro.TipoAprobador, cancellationToken))
             .ToDictionary(
                 x => x.Id,
                 x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
             );

        return PlantillaFactory.CrearDesdeRequest(
            input,
            idPlantilla,
            idEtapa,
            idFirmante,
            usuarioActual,
            gerenciaUsuario,
            tipoEtapaDict,
            tipoAprobadorDict
        );
    }
}
