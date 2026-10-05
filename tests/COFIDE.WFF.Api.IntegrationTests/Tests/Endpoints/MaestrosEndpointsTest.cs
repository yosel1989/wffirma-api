using System.Net;
using COFIDE.WFF.Api.IntegrationTests.Support;
using FluentAssertions;

namespace COFIDE.WFF.Api.IntegrationTests.Tests.Endpoints;

public sealed record EndpointLecturaCase(string Caso, string Ruta)
{
    public override string ToString() => Caso;
}

/// <summary>Punta a punta: HTTP → controlador → MediatR → handler → repositorio → Oracle.</summary>
[Collection(ColeccionIntegracion.Nombre)]
[Trait("Categoria", "Integracion")]
public class MaestrosEndpointsTest(WorkflowApiFactory api)
{
    public static IEnumerable<object[]> Endpoints =>
        JsonResource.Cases<EndpointLecturaCase>("Endpoints/maestros-lectura.json");

    [TheoryConBaseDeDatos]
    [MemberData(nameof(Endpoints))]
    public async Task Endpoint_De_Consulta_Debe_Responder_Sin_Error_De_Servidor(EndpointLecturaCase caso)
    {
        using var cliente = api.ClienteAutenticado();

        var respuesta = await cliente.GetAsync(caso.Ruta);

        // 404 es válido: estos endpoints responden NotFound cuando el catálogo está vacío.
        respuesta.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [TheoryConBaseDeDatos]
    [MemberData(nameof(Endpoints))]
    public async Task Endpoint_Sin_Autenticacion_No_Debe_Devolver_Datos(EndpointLecturaCase caso)
    {
        using var cliente = api.CreateClient();

        var respuesta = await cliente.GetAsync(caso.Ruta);

        respuesta.StatusCode.Should().NotBe(HttpStatusCode.OK);
        ((int)respuesta.StatusCode).Should().BeLessThan(500);
    }
}
