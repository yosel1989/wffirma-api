using COFIDE.WFF.Api.Middleware;
using COFIDE.WFF.Application.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Api.Tests.Tests.Middleware;

public class ExceptionHandlingMiddlewareApplicationTest
{
    [Fact]
    public async Task Application_PlantillaYaNoExiste_DevuelveNotFound_RF01()
    {
        var context = CrearContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.PlantillaYaNoExiste),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task Application_SinCodigoHttpExplicito_MantieneBadRequest_RF01()
    {
        var context = CrearContexto();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ValidacionAplicacion.ApplicationException(
                ApplicationErrors.PlantillaInvalid),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    private static DefaultHttpContext CrearContexto()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
