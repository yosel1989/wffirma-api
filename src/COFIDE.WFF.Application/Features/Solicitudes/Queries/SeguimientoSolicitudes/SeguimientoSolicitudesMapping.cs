using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

public static class SeguimientoSolicitudesMapping
{
    /// <summary>Agrupa en un solo elemento los firmantes consecutivos de una misma etapa paralela.</summary>
    public static List<FirmantesDto> AgruparFirmantes(List<FirmantesDto>? firmantes)
    {
        if (firmantes is null || firmantes.Count == 0)
            return [];

        var resultado = new List<FirmantesDto>();
        var indice = 0;

        while (indice < firmantes.Count)
        {
            var actual = firmantes[indice];
            var esParalelo = actual.CodigoTipoEtapa == TipoEtapa.Paralelo.Codigo;

            if (!esParalelo)
            {
                resultado.Add(new FirmantesDto
                {
                    Nombre = actual.Nombre,
                    Estado = actual.Estado
                });
                indice++;
                continue;
            }

            var paralelos = new List<FirmantesParaleloDto>();
            var orden = actual.OrdenEtapa;

            while (indice < firmantes.Count
                   && firmantes[indice].CodigoTipoEtapa == TipoEtapa.Paralelo.Codigo
                   && firmantes[indice].OrdenEtapa == orden)
            {
                paralelos.Add(new FirmantesParaleloDto
                {
                    Nombre = firmantes[indice].Nombre,
                    Estado = firmantes[indice].Estado
                });
                indice++;
            }

            resultado.Add(new FirmantesDto
            {
                Paralelo = paralelos
            });
        }

        return resultado;
    }
}
