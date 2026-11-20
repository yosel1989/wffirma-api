using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Entities;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;

public class PlantillaFactoryTest
{
    [Fact]
    public void CrearDesdeRequest_AsignaAreaYAreaPadreRecibidos_RF01()
    {
        var input = new CrearPlantillaInput
        {
            NombrePlantilla = "Plantilla de prueba",
            Descripcion = "Descripción",
            Visibilidad = false,
            EsReutilizable = false,
            Etapas = []
        };

        var plantilla = PlantillaFactory.CrearDesdeRequest(
            input,
            10,
            20,
            30,
            40,
            "Gerencia",
            new Dictionary<long, ParametroDetalle>(),
            new Dictionary<long, ParametroDetalle>(),
            idAreaPlantilla: 101,
            idAreaPadrePlantilla: 202);

        plantilla.IdAreaPlantilla.Should().Be(101);
        plantilla.IdAreaPadrePlantilla.Should().Be(202);
    }
}
