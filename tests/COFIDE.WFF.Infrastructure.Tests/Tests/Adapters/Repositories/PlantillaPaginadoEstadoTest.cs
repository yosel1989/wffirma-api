using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public class PlantillaPaginadoEstadoTest
{
    [Fact]
    public async Task ObtenerPaginado_ConActivaInactivaYEliminada_FiltraAntesDePaginar_RF04()
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

        var resultado = await repository.ObtenerPaginado(new PlantillaPaginadoRequest
        {
            PaginationParams = new PagedQueryParams
            {
                PageNumber = 1,
                PageSize = 1
            },
            Visibilidad = false,
            RolUsuario = RolUsuario.Administrador,
            AreaParaCoordinador = string.Empty,
            IdUsuarioPropietario = 10
        });

        Assert.Equal(2, resultado.TotalCount);
        Assert.DoesNotContain(resultado.Items, item => item.IdPlantilla == 3);
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
