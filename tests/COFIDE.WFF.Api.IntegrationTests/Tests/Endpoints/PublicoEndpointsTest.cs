using System.Net;
using COFIDE.WFF.Api.IntegrationTests.Support;
using FluentAssertions;

namespace COFIDE.WFF.Api.IntegrationTests.Tests.Endpoints;

/// <summary>Descarga pública por token (enlace enviado en el correo de aprobación, B1).</summary>
[Collection(ColeccionIntegracion.Nombre)]
[Trait("Categoria", "Integracion")]
public class PublicoEndpointsTest(WorkflowApiFactory api)
{
    [FactConBaseDeDatos]
    public async Task Token_Inexistente_Debe_Responder_Error_De_Negocio_Y_No_Error_De_Servidor()
    {
        using var cliente = api.CreateClient();

        var respuesta = await cliente.GetAsync($"/api/v1/public/token-inexistente-{Guid.NewGuid():N}");
        var cuerpo = await respuesta.Content.ReadAsStringAsync();

        ((int)respuesta.StatusCode).Should().BeInRange(400, 499);
        cuerpo.Should().Contain("APP_SOL_010");
    }
}
