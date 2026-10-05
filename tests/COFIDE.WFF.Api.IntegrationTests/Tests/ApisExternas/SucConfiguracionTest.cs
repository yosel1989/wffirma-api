using COFIDE.WFF.Api.IntegrationTests.Support;
using COFIDE.WFF.Infrastructure.Configuracion.Runtime;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace COFIDE.WFF.Api.IntegrationTests.Tests.ApisExternas;

/// <summary>
/// Pruebas de humo contra el SUC real (solo lectura). Confirman que la Api puede obtener su configuración
/// en el ambiente donde se ejecutan. Deshabilitadas por defecto; activar únicamente en QA.
/// Nunca imprimen los valores obtenidos.
/// </summary>
[Collection(ColeccionIntegracion.Nombre)]
[Trait("Categoria", "ApisExternas")]
public class SucConfiguracionTest(WorkflowApiFactory api)
{
    [FactConApisExternas]
    public async Task Debe_Obtener_La_Cadena_De_Conexion_Desde_El_SUC()
    {
        await using var scope = api.CrearScope();
        var runtime = scope.ServiceProvider.GetRequiredService<IRuntimeConfigurationProvider>();

        var cadena = await runtime.GetOracleConnectionAsync();

        cadena.Should().NotBeNullOrWhiteSpace();
    }

    [FactConApisExternas]
    public async Task Debe_Obtener_La_Clave_JWT_Desde_El_SUC()
    {
        await using var scope = api.CrearScope();
        var runtime = scope.ServiceProvider.GetRequiredService<IRuntimeConfigurationProvider>();

        var clave = await runtime.GetJwtSecretAsync();

        clave.Should().NotBeNullOrWhiteSpace();
    }
}
