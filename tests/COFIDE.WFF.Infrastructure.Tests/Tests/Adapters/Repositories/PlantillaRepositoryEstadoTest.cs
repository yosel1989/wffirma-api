using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public class PlantillaRepositoryEstadoTest
{
    [Fact]
    public async Task ObtenerParaCambioEstado_ConEliminada_DevuelveRaizSinRelaciones_RF01()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var seed = CrearContexto(databaseName))
        {
            var plantilla = Plantilla.Crear(
                100, "Plantilla", "Descripción", false, 10, "GTI", true);
            plantilla.AgregarEtapas(
                200, 1, "Etapa", TipoEtapa.Secuencial, 1, TipoAprobador.Firmante, 10);
            plantilla.EliminarLogicamente();
            seed.Plantillas.Add(plantilla);
            await seed.SaveChangesAsync();
        }

        using var context = CrearContexto(databaseName);
        var repository = new PlantillaRepository(context);

        var resultado = await repository.ObtenerParaCambioEstadoAsync(
            100,
            CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(EstadoPlantilla.Eliminada, resultado.EstadoPlantilla);
        Assert.Empty(resultado.Etapas);
    }

    [Fact]
    public async Task ObtenerParaCambioEstado_ConIdInexistente_DevuelveNull_RF01()
    {
        using var context = CrearContexto(Guid.NewGuid().ToString());
        var repository = new PlantillaRepository(context);

        var resultado = await repository.ObtenerParaCambioEstadoAsync(
            999,
            CancellationToken.None);

        Assert.Null(resultado);
    }

    private static WorkflowFirmaDbContext CrearContexto(string databaseName)
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new WorkflowFirmaDbContext(options, null!);
    }
}
