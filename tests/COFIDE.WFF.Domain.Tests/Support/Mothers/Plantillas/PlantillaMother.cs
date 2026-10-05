using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;

internal static class PlantillaMother
{
    public static Plantilla Vacia(long id = 1, bool? publica = false, bool? reutilizable = true) =>
        Plantilla.Crear(id, "Flujo Aprobacion", "Descripcion", publica, 10, "GTI", reutilizable);

    /// <summary>Plantilla con N etapas secuenciales consecutivas, un firmante cada una.</summary>
    public static Plantilla ConEtapasSecuenciales(params long[] ordenes)
    {
        var plantilla = Vacia();
        long etapaId = 1, firmanteId = 1, usuarioId = 100;

        foreach (var orden in ordenes)
        {
            var etapa = plantilla.AgregarEtapas(
                etapaId, orden, $"Etapa {orden}",
                Vo.TipoEtapa.Secuencial, idTipoEtapa: 1,
                Vo.TipoAprobador.Firmante, idTipoAprobador: 10);

            etapa.AgregarFirmante(firmanteId++, usuarioId++, "Jefe", "GTI", "GTI", 10);
            etapaId++;
        }

        return plantilla;
    }
}
