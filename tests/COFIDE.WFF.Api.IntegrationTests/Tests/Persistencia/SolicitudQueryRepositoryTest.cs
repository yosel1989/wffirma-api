using COFIDE.WFF.Api.IntegrationTests.Support;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Pageds;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace COFIDE.WFF.Api.IntegrationTests.Tests.Persistencia;

/// <summary>
/// H3 contra Oracle: las consultas de lectura se movieron a SolicitudQueryRepository.
/// Estas pruebas verifican que EF Core las traduce a SQL válido para Oracle (en memoria no se puede comprobar).
/// Usan un usuario inexistente para no depender de los datos de la base de pruebas.
/// </summary>
[Collection(ColeccionIntegracion.Nombre)]
[Trait("Categoria", "Integracion")]
public class SolicitudQueryRepositoryTest(WorkflowApiFactory api)
{
    private const long UsuarioInexistente = -999;

    [FactConBaseDeDatos]
    public async Task Paginado_Con_Todos_Los_Filtros_Debe_Ejecutarse()
    {
        await using var scope = api.CrearScope();
        var consultas = scope.ServiceProvider.GetRequiredService<ISolicitudQueryRepository>();

        var resultado = await consultas.ObtenerPaginado(
            new PagedQueryParams(), UsuarioInexistente, 1, "SOLAPR", 1,
            "2020-01-01", "2030-12-31",
            [1], [1], [1], [1], [1], [1], [1], [1],
            "SOL", "asunto", "REF", urgente: true, usuario: true, usuariodepartamento: false, incluirresumen: true,
            CancellationToken.None);

        resultado.Should().NotBeNull();
    }

    [TheoryConBaseDeDatos]
    [InlineData("COORD")]
    [InlineData("SOLAPR")]
    [InlineData(null)]
    public async Task Paginado_Debe_Ejecutarse_Para_Cada_Rol(string? rol)
    {
        await using var scope = api.CrearScope();
        var consultas = scope.ServiceProvider.GetRequiredService<ISolicitudQueryRepository>();

        var resultado = await consultas.ObtenerPaginado(
            new PagedQueryParams(), UsuarioInexistente, 1, rol, 1, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, incluirresumen: true,
            CancellationToken.None);

        resultado.Should().NotBeNull();
    }

    [FactConBaseDeDatos]
    public async Task Bandeja_De_Entrada_Debe_Ejecutarse_Y_Estar_Vacia_Para_Un_Usuario_Inexistente()
    {
        await using var scope = api.CrearScope();
        var consultas = scope.ServiceProvider.GetRequiredService<ISolicitudQueryRepository>();

        var resultado = await consultas.ObtenerBandejaEntrada(
            new PagedQueryParams(), UsuarioInexistente, null, null,
            null, null, null, null, null, null,
            null, null, null, null, incluirresumen: true,
            CancellationToken.None);

        resultado.Should().NotBeNull();
    }

    [FactConBaseDeDatos]
    public async Task Panel_De_Control_Debe_Ejecutarse()
    {
        await using var scope = api.CrearScope();
        var consultas = scope.ServiceProvider.GetRequiredService<ISolicitudQueryRepository>();

        var act = () => consultas.ObtenerSolicitudPanelControl(UsuarioInexistente, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [FactConBaseDeDatos]
    public async Task Consultas_Puntuales_Deben_Ejecutarse()
    {
        await using var scope = api.CrearScope();
        var consultas = scope.ServiceProvider.GetRequiredService<ISolicitudQueryRepository>();

        (await consultas.ObtenerParaNotificaciones(-1, CancellationToken.None)).Should().BeNull();
        (await consultas.ObtenerCodigosExistentesAsync(null, ["CODIGO-INEXISTENTE-IT"], CancellationToken.None)).Should().BeEmpty();
        (await consultas.ObtenerDocumentosFirmadosPorSolicitudAsync(-1, CancellationToken.None)).Should().BeEmpty();
    }
}
