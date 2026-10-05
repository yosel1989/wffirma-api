using System.Reflection;
using COFIDE.WFF.Api.IntegrationTests.Support;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace COFIDE.WFF.Api.IntegrationTests.Tests.Persistencia;

/// <summary>
/// Verifica que el modelo de EF Core coincide con la base de datos real: cada entidad mapeada
/// se consulta una vez. Detecta tablas o columnas faltantes, nombres mal escritos y tipos incompatibles
/// (por ejemplo, una columna nueva en la entidad que todavía no existe en la base de pruebas).
/// </summary>
[Collection(ColeccionIntegracion.Nombre)]
[Trait("Categoria", "Integracion")]
public class MapeoEntidadesTest(WorkflowApiFactory api)
{
    [FactConBaseDeDatos]
    public async Task La_Base_De_Datos_De_Pruebas_Debe_Estar_Disponible()
    {
        await using var scope = api.CrearScope();
        var contexto = scope.ServiceProvider.GetRequiredService<WorkflowFirmaDbContext>();

        (await contexto.Database.CanConnectAsync()).Should().BeTrue();
    }

    [FactConBaseDeDatos]
    public async Task Cada_Entidad_Mapeada_Debe_Poder_Consultarse()
    {
        await using var scope = api.CrearScope();
        var contexto = scope.ServiceProvider.GetRequiredService<WorkflowFirmaDbContext>();
        var consultar = typeof(MapeoEntidadesTest).GetMethod(nameof(ConsultarPrimeroAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

        var errores = new List<string>();
        var entidades = contexto.Model.GetEntityTypes()
            .Where(e => !e.IsOwned() && e.FindPrimaryKey() is not null && !e.HasSharedClrType)
            .Select(e => e.ClrType)
            .Distinct();

        foreach (var tipo in entidades)
        {
            try
            {
                await (Task)consultar.MakeGenericMethod(tipo).Invoke(null, [contexto])!;
            }
            catch (Exception ex)
            {
                errores.Add($"{tipo.Name} ({contexto.Model.FindEntityType(tipo)?.GetTableName()}): {ex.GetBaseException().Message}");
            }
        }

        errores.Should().BeEmpty("todas las entidades del modelo deben existir en la base con sus columnas");
    }

    private static Task ConsultarPrimeroAsync<T>(WorkflowFirmaDbContext contexto) where T : class =>
        contexto.Set<T>().AsNoTracking().Take(1).ToListAsync();
}
