using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public class PlantillaGestionEstadoTest
{
    [Fact]
    public async Task ObtenerPaginadoGestion_CombinaEstadosAlcanceYFiltrosAntesDePaginar_RF05()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var seed = CrearContexto(databaseName))
        {
            var activa = CrearPlantilla(1, "Coincide Activa", 100, 200);
            var inactiva = CrearPlantilla(2, "Coincide Inactiva", 100, 200);
            inactiva.Desactivar();
            var eliminada = CrearPlantilla(3, "Coincide Eliminada", 100, 200);
            eliminada.EliminarLogicamente();
            var fueraAlcance = CrearPlantilla(4, "Coincide Fuera", 101, 201);

            seed.Plantillas.AddRange(activa, inactiva, eliminada, fueraAlcance);
            foreach (var plantilla in new[] { activa, inactiva, eliminada, fueraAlcance })
            {
                var (departamento, gerencia) = plantilla.Id switch
                {
                    4 => (101L, 201L),
                    _ => (100L, 200L)
                };
                seed.Entry(plantilla).Property(p => p.IdAreaPlantilla).CurrentValue = departamento;
                seed.Entry(plantilla).Property(p => p.IdAreaPadrePlantilla).CurrentValue = gerencia;
            }
            await seed.SaveChangesAsync();
        }

        using var context = CrearContexto(databaseName);
        var repository = new PlantillaGestionRepository(context);
        var parametros = new PagedQueryParams { PageNumber = 1, PageSize = 1 };

        var primera = await repository.ObtenerPaginadoGestion(
            parametros, 10, 100, 200, RolUsuarioCodigo.Coordinador,
            null, null, [200], [100], [10], null, "Coincide", null,
            CancellationToken.None);
        parametros.PageNumber = 2;
        var segunda = await repository.ObtenerPaginadoGestion(
            parametros, 10, 100, 200, RolUsuarioCodigo.Coordinador,
            null, null, [200], [100], [10], null, "Coincide", null,
            CancellationToken.None);

        Assert.Equal(2, primera.TotalCount);
        Assert.Equal(2, segunda.TotalCount);
        Assert.Equal([1L, 2L],
            primera.Items.Concat(segunda.Items)
                .Select(item => item.IdPlantilla)
                .OrderBy(id => id)
                .ToArray());
        primera.Items.Concat(segunda.Items).Single(item => item.IdPlantilla == 1).Activo.Should().BeTrue();
        primera.Items.Concat(segunda.Items).Single(item => item.IdPlantilla == 2).Activo.Should().BeFalse();
    }

    [Fact]
    public void Proyectar_NoGeneraLiteralesBooleanosNoValidosEnOracle()
    {
        var opciones = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseOracle("Data Source=sin-conexion")
            .Options;
        using var context = new WorkflowFirmaDbContext(opciones, null!);
        var repository = new PlantillaGestionRepository(context);
        var proyectar = typeof(PlantillaGestionRepository)
            .GetMethod("Proyectar", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var query = (IQueryable)proyectar.Invoke(repository, [context.Plantillas.AsNoTracking()])!;
        var sql = query.ToQueryString();

        sql.Should().NotMatchRegex(@"\b(FALSE|TRUE)\b");
        sql.Should().MatchRegex(@"CASE\s+WHEN\b[\s\S]*\bTHEN 1\s+ELSE 0\s+END");
    }

    private static Plantilla CrearPlantilla(
        long id,
        string nombre,
        long departamento,
        long gerencia)
    {
        var plantilla = Plantilla.Crear(
            id, nombre, "Descripción", false, 10, gerencia.ToString(), true);
        return plantilla;
    }

    private static WorkflowFirmaDbContext CrearContexto(string databaseName)
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new WorkflowFirmaDbContext(options, null!);
    }
}
