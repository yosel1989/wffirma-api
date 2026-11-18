using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;

namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;

public class PlantillaEliminacionLogicaTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EliminarLogicamente_ConPlantillaActivaOInactiva_ConservaDatosYFlujo_RF01(
        bool activa)
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1);
        if (!activa)
            plantilla.Desactivar();

        var nombre = plantilla.NombrePlantilla;
        var descripcion = plantilla.Descripcion;
        var etapa = plantilla.Etapas.Single();
        var firmante = etapa.Firmantes.Single();

        plantilla.EliminarLogicamente();

        Assert.Equal(EstadoPlantilla.Eliminada, plantilla.EstadoPlantilla);
        Assert.False(plantilla.Estado);
        Assert.Same(nombre, plantilla.NombrePlantilla);
        Assert.Same(descripcion, plantilla.Descripcion);
        Assert.Same(etapa, Assert.Single(plantilla.Etapas));
        Assert.Same(firmante, Assert.Single(etapa.Firmantes));
    }
}
