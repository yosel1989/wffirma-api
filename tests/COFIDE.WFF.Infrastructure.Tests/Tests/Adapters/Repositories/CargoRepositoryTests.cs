using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using COFIDE.WFF.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

/// <summary>Pruebas del repositorio del combo de cargos.</summary>
public class CargoRepositoryTests
{
    /// <summary>Comprueba la proyección y el orden de cargos activos e inactivos.</summary>
    [Fact]
    public async Task GetCargoCombo_DevuelveTodosLosCargosOrdenadosPorNombre()
    {
        await using var context = CrearContexto();
        context.Set<Cargo>().AddRange(
            new Cargo().Con(c => c.Id, 10L).Con(c => c.NombreCargo, "Jefe").Con(c => c.Estado, true),
            new Cargo().Con(c => c.Id, 20L).Con(c => c.NombreCargo, "Analista").Con(c => c.Estado, false));
        await context.SaveChangesAsync();

        var result = await new CargoRepository(context).GetCargoCombo(CancellationToken.None);

        Assert.Collection(result,
            cargo => { Assert.Equal(20L, cargo.IdCargo); Assert.Equal("Analista", cargo.NombreCargo); },
            cargo => { Assert.Equal(10L, cargo.IdCargo); Assert.Equal("Jefe", cargo.NombreCargo); });
    }

    /// <summary>Comprueba que una tabla vacía produce una lista vacía.</summary>
    [Fact]
    public async Task GetCargoCombo_SinCargos_DevuelveListaVacia()
    {
        await using var context = CrearContexto();

        var result = await new CargoRepository(context).GetCargoCombo(CancellationToken.None);

        Assert.Empty(result);
    }

    /// <summary>Crea un contexto independiente para cada prueba.</summary>
    private static WorkflowFirmaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WorkflowFirmaDbContext(options, null!);
    }
}
