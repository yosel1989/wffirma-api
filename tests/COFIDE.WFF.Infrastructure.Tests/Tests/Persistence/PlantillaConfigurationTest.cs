using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Persistence;

public class PlantillaConfigurationTest
{
    [Fact]
    public void Modelo_Plantilla_PersisteSoloEstadoTriarioEnChEstado_RF01()
    {
        using var context = CrearContexto();
        var entity = context.Model.FindEntityType(typeof(Plantilla));

        Assert.NotNull(entity);
        Assert.Null(entity.FindProperty(nameof(Plantilla.Estado)));

        var estado = entity!.FindProperty(nameof(Plantilla.EstadoPlantilla));
        Assert.NotNull(estado);
        Assert.Equal("CH_ESTADO", estado!.GetColumnName());
        Assert.True(estado.IsConcurrencyToken);

        var propiedadesEstado = entity.GetProperties()
            .Where(propiedad => propiedad.GetColumnName() == "CH_ESTADO")
            .ToList();
        Assert.Single(propiedadesEstado);

        var converter = estado.GetValueConverter();
        Assert.NotNull(converter);
        Assert.Equal("A", converter!.ConvertToProvider(EstadoPlantilla.Activa));
        Assert.Equal("I", converter.ConvertToProvider(EstadoPlantilla.Inactiva));
        Assert.Equal("E", converter.ConvertToProvider(EstadoPlantilla.Eliminada));
    }

    private static WorkflowFirmaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WorkflowFirmaDbContext(options, null!);
    }
}
