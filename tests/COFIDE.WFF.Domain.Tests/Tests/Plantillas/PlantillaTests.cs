using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;

public class PlantillaTest
{

    [Fact]
    public void Crear_Deberia_Crear_Plantilla_Valida()
    {
        var plantilla = Plantilla.Crear(
            1,
            "Flujo Aprobacion",
            "Descripcion",
            true,
            10,
            "",
            true
        );

        Assert.NotNull(plantilla);
        Assert.Equal("Flujo Aprobacion", plantilla.NombrePlantilla.Valor);
    }

   /* [Fact]
    public void AgregarEtapa_OrdenDuplicado_Deberia_Fallar()
    {
        var plantilla = Plantilla.Crear(1, "P", null, true, 10, "", true);

        plantilla.AgregarEtapas(1, 1, "Etapa 1", 1, 1);

        // Validar si la falla es correcta
        Assert.Throws<DomainException>(() =>
           plantilla.AgregarEtapas(2, 3, "Etapa 2", 1, 1)
        );

        // Validar si la falla es correcta con el Error correspondientes
        /*var exception = Assert.Throws<DomainException>(() =>
            plantilla.AgregarEtapas(2, 3, "Etapa 2", 1, 1)
        );

        Assert.Equal(PlantillaErrors.SecuenciaEtapasInvalida, exception.Error);
    }*/
}
