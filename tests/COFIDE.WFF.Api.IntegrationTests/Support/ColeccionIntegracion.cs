namespace COFIDE.WFF.Api.IntegrationTests.Support;

/// <summary>
/// Todas las clases de prueba comparten una sola instancia de la Api (arranque costoso)
/// y se ejecutan en serie para no competir por los mismos datos de la base de pruebas.
/// </summary>
[CollectionDefinition(Nombre)]
public sealed class ColeccionIntegracion : ICollectionFixture<WorkflowApiFactory>
{
    public const string Nombre = "Integracion";
}
