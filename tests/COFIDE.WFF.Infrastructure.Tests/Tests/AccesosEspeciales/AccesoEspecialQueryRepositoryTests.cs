using Microsoft.EntityFrameworkCore;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Infrastructure.Tests.Helpers;

public class AccesoEspecialQueryRepositoryTests
{
    private WorkflowFirmaDbContext CrearDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new WorkflowFirmaDbContext(options, null!);
    }

    [Fact]
    public async Task ObtenerPaginado_DevuelveResultadosFiltradosYPaginados()
    {
        // Arrange
        var dbName = nameof(ObtenerPaginado_DevuelveResultadosFiltradosYPaginados);
        using var context = CrearDbContext(dbName);

        // Crea el usuario usando el constructor público
        var usuario = new Usuario(1, RolUsuario.Administrador)
        .Con(u => u.Nombres, "Juan")
        .Con(u => u.ApellidoPaterno, "Pérez")
        .Con(u => u.ApellidoMaterno, "Gómez")
        .Con(u => u.CuentaLogueo, "juanp");
        context.Usuarios.Add(usuario);

        var area = new Area()
        .Con(u => u.Id, 10)
        .Con(u => u.CodDependencia, "A01")
        .Con(u => u.NombreArea, "Gerencia")
        .Con(u => u.DescripcionCorta, "G");
        context.Areas.Add(area);

        // Crea el AccesoEspecial usando el método de fábrica
        var accesoEspecial = AccesoEspecial.Crear(100, 1, 10);
        accesoEspecial
            .Con(u => u.Usuario, usuario)
            .Con(u => u.Area, area)
            .Con(u => u.Estado, true)
            .Con(u => u.FechaCreacion, System.DateTime.UtcNow);
        context.AccesosEspeciales.Add(accesoEspecial);

        context.SaveChanges();

        var repo = new AccesoEspecialQueryRepository(context);

        var pagedParams = new PagedQueryParams
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await repo.ObtenerPaginado(pagedParams, 10, 1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        var item = result.Items.First();
        Assert.Equal(100, item.IdAccesoEspecial);
        Assert.Equal("Juan Pérez Gómez", item.NombreUsuario);
        Assert.Equal("juanp", item.NombreCuenta);
        Assert.Equal("Gerencia", item.NombreArea);
        Assert.Equal("A01", item.CodigoArea);
        Assert.True(item.Estado);
    }

    [Fact]
    public async Task ObtenerPaginado_SinResultados_RetornaColeccionVacia()
    {
        // Arrange
        var dbName = nameof(ObtenerPaginado_SinResultados_RetornaColeccionVacia);
        using var context = CrearDbContext(dbName);

        var repo = new AccesoEspecialQueryRepository(context);

        var pagedParams = new PagedQueryParams
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await repo.ObtenerPaginado(pagedParams, 99, 99, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.NotNull(result.ExtraData);
        Assert.Equal(0, result.ExtraData.Total);
    }
}