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
            "Eliminada", false, 10, 100, 200));
        Assert.NotNull(await repository.ValidarExistenciaNombrePlantilla(
            "Inactiva", false, 10, 100, 200));
    }

    [Fact]
    public async Task ValidarExistencia_ConPrivadaBuscaLaMismaAreaYGerencia_RF01()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var seed = CrearContexto(databaseName))
        {
            var privadaOtraArea = CrearPlantilla(1, "Duplicada", 300, 200, 11);
            var privadaMismaArea = CrearPlantilla(2, "Duplicada", 100, 200, 11);
            seed.Plantillas.AddRange(privadaOtraArea, privadaMismaArea);
            await seed.SaveChangesAsync();
        }

        using var context = CrearContexto(databaseName);
        var repository = new PlantillaRepository(context);

        var resultado = await repository.ValidarExistenciaNombrePlantilla(
            "Duplicada", false, 10, 100, 200);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Id);
    }

    [Fact]
    public async Task ValidarExistencia_ConPrivadaBuscaPorUsuarioCreador_RF01()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var seed = CrearContexto(databaseName))
        {
            var privadaPropia = CrearPlantilla(1, "Duplicada", 300, 400);
            seed.Plantillas.Add(privadaPropia);
            await seed.SaveChangesAsync();
        }

        using var context = CrearContexto(databaseName);
        var repository = new PlantillaRepository(context);

        var resultado = await repository.ValidarExistenciaNombrePlantilla(
            "Duplicada", false, 10, 100, 200);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
    }

    [Fact]
    public async Task ValidarExistencia_ConPublicaBuscaEntreTodas_RF01()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var seed = CrearContexto(databaseName))
        {
            var publica = Plantilla.Crear(
                1, "Duplicada", "Descripción", true, 10, "GTI", true, 300, 400);
            seed.Plantillas.Add(publica);
            await seed.SaveChangesAsync();
        }

        using var context = CrearContexto(databaseName);
        var repository = new PlantillaRepository(context);

        var resultado = await repository.ValidarExistenciaNombrePlantilla(
            "Duplicada", true, 10, 100, 200);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
    }

    private static Plantilla CrearPlantilla(
        long id,
        string nombre,
        long? idArea = 100,
        long? idAreaPadre = 200,
        long idUsuarioPropietario = 10) =>
        Plantilla.Crear(
            id,
            nombre,
            "Descripción",
            false,
            idUsuarioPropietario,
            "GTI",
            true,
            idArea,
            idAreaPadre);

    private static WorkflowFirmaDbContext CrearContexto(string databaseName)
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new WorkflowFirmaDbContext(options, null!);
    }
}
