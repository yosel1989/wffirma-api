using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public class PlantillaRepositoryDisponibilidadTest
{
    [Fact]
    public async Task ConsultasDisponibles_ConActivaInactivaYEliminada_ExcluyenSoloEliminada_RF01_RF04()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var seed = CrearContexto(databaseName))
        {
            var activa = CrearPlantilla(1, "Activa");
            var inactiva = CrearPlantilla(2, "Inactiva");
            inactiva.Desactivar();
            var eliminada = CrearPlantilla(3, "Eliminada");
            eliminada.EliminarLogicamente();

            seed.Plantillas.AddRange(activa, inactiva, eliminada);
            await seed.SaveChangesAsync();
        }

        using var context = CrearContexto(databaseName);
        var repository = new PlantillaRepository(context);

        Assert.Null(await repository.GetByIdAsync(3));
        Assert.NotNull(await repository.GetByIdAsync(2));

        var todas = await repository.GetAllAsync();
        Assert.Equal([1L, 2L], todas.Select(p => p.Id).OrderBy(id => id).ToArray());

        Assert.Null(await repository.ObtenerAgregadoPlantillaAsync(
            3, CancellationToken.None));
        Assert.NotNull(await repository.ObtenerAgregadoPlantillaAsync(
            2, CancellationToken.None));

        Assert.Null(await repository.ObtenerDetallePlantillaPorId(3));
        Assert.NotNull(await repository.ObtenerDetallePlantillaPorId(2));

        Assert.Null(await repository.ValidarExistenciaNombrePlantilla(
            "Eliminada", false, 10));
        Assert.NotNull(await repository.ValidarExistenciaNombrePlantilla(
            "Inactiva", false, 10));
    }

    private static Plantilla CrearPlantilla(long id, string nombre) =>
        Plantilla.Crear(id, nombre, "Descripción", false, 10, "GTI", true);

    private static WorkflowFirmaDbContext CrearContexto(string databaseName)
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new WorkflowFirmaDbContext(options, null!);
    }
}
