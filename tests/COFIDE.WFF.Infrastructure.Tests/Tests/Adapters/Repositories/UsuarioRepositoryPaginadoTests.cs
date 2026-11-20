using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using COFIDE.WFF.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

/// <summary>Verifica la proyección y el paginado del listado de usuarios.</summary>
public sealed class UsuarioRepositoryPaginadoTests
{
    /// <summary>Comprueba los siete campos del usuario inactivo que cumple todos los filtros.</summary>
    [Fact]
    public async Task ObtenerPaginadoAsync_FiltrosCombinados_DevuelveUsuarioInactivoConSuJerarquia()
    {
        await using var context = await CrearContextoAsync();
        var repository = new UsuarioRepository(context);

        var result = await repository.ObtenerPaginadoAsync(new(), [10], [20], 40, false, 1, default);
        var item = Assert.Single(result.Items);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1L, item.IdUsuario);
        Assert.Equal("ana", item.CuentaLogueo);
        Assert.Equal("Ana Perez Soto", item.NombreCompleto);
        Assert.Equal("Gerencia", item.NombreGerencia);
        Assert.Equal("Departamento", item.NombreDepartamento);
        Assert.Equal("Analista", item.NombreCargo);
        Assert.False(item.Estado);
    }

    /// <summary>Comprueba el orden, la inclusión de usuarios sin asignaciones y el total fuera de la última página.</summary>
    [Fact]
    public async Task ObtenerPaginadoAsync_SinFiltros_OrdenaYPaginaTodosLosUsuarios()
    {
        await using var context = await CrearContextoAsync();
        var repository = new UsuarioRepository(context);
        var paginacion = new PagedQueryParams { PageSize = 1, Sorts = [("nombreCompleto", true)] };

        var result = await repository.ObtenerPaginadoAsync(paginacion, null, null, null, null, null, default);
        var item = Assert.Single(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2L, item.IdUsuario);
        Assert.True(item.Estado);
        Assert.Null(item.NombreGerencia);
        Assert.Null(item.NombreDepartamento);
        Assert.Null(item.NombreCargo);

        paginacion.PageNumber = 3;
        var ultima = await repository.ObtenerPaginadoAsync(paginacion, null, null, null, null, null, default);
        Assert.Empty(ultima.Items);
        Assert.Equal(2, ultima.TotalCount);
        Assert.Equal(3, ultima.PageNumber);
    }

    /// <summary>Sin departamentos, filtra por gerencia incluyendo usuarios de sus áreas hijas.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ObtenerPaginadoAsync_SoloGerencias_IncluyeUsuariosDeAreasHijas(bool departamentosVacio)
    {
        await using var context = await CrearContextoAsync();
        var repository = new UsuarioRepository(context);

        var result = await repository.ObtenerPaginadoAsync(
            new(), [999, 10], departamentosVacio ? [] : null, null, null, null, default);

        Assert.Equal(1L, Assert.Single(result.Items).IdUsuario);
    }

    /// <summary>Con departamentos, la gerencia enviada se ignora.</summary>
    [Fact]
    public async Task ObtenerPaginadoAsync_ConDepartamentos_IgnoraGerencias()
    {
        await using var context = await CrearContextoAsync();
        var repository = new UsuarioRepository(context);

        var coincide = await repository.ObtenerPaginadoAsync(new(), [999], [20], null, null, null, default);
        var noCoincide = await repository.ObtenerPaginadoAsync(new(), [10], [999], null, null, null, default);

        Assert.Equal(1L, Assert.Single(coincide.Items).IdUsuario);
        Assert.Empty(noCoincide.Items);
    }

    /// <summary>Prepara una jerarquía de tres niveles y usuarios con y sin asignaciones.</summary>
    private static async Task<WorkflowFirmaDbContext> CrearContextoAsync()
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), options => options.EnableNullChecks(false))
            .Options;
        var context = new WorkflowFirmaDbContext(options, null!);
        var gerencia = CrearArea(10, 'G', "Gerencia");
        var departamento = CrearArea(20, 'D', "Departamento").Con(a => a.AreaPadre, gerencia);
        var area = CrearArea(30, 'S', "Subarea").Con(a => a.AreaPadre, departamento);
        var cargo = new Cargo().Con(c => c.Id, 40L).Con(c => c.NombreCargo, "Analista");
        var ana = CrearUsuario(1, "Ana", false)
            .Con(u => u.CuentaLogueo, "ana").Con(u => u.Area, area).Con(u => u.Cargo, cargo);
        context.Usuarios.AddRange(ana, CrearUsuario(2, "Bruno", true));
        await context.SaveChangesAsync();
        return context;
    }

    /// <summary>Crea un área para la jerarquía de prueba.</summary>
    private static Area CrearArea(long id, char tipo, string nombre)
    {
        var area = new Area().Con(a => a.Id, id).Con(a => a.NombreArea, nombre);
        typeof(Area).GetField("<Tipo>k__BackingField", System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)!.SetValue(area, tipo);
        return area;
    }

    /// <summary>Crea un usuario con identidad y estado definidos.</summary>
    private static Usuario CrearUsuario(long id, string nombre, bool estado) =>
        new Usuario(id, RolUsuario.Administrador)
            .Con(u => u.Nombres, nombre).Con(u => u.ApellidoPaterno, "Perez")
            .Con(u => u.ApellidoMaterno, "Soto").Con(u => u.Estado, estado);
}
