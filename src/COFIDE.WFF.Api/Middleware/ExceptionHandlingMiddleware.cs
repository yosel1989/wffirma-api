using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Infrastructure.Exceptions;
namespace COFIDE.WFF.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidation(context, ex);
        }
        catch (ApplicationExceptionBase ex)
        {
            await HandleApplication(context, ex);
        }
        catch (DomainExceptionBase ex)
        {
            await HandleDomain(context, ex);
        }
        catch (InfrastructureExceptionBase ex)
        {
            await HandleInfrastructure(context, ex);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var custom = new ValidationException(ex.Errors);
            await HandleValidation(context, custom);
        }
        catch (Exception ex)
        {
            await HandleUnhandled(context, ex);
        }
    }

    private static async Task HandleValidation(
        HttpContext context,
        ValidationException ex)
    {
        var errors = ex.Errors.Select(e =>
            new ErrorDto(e.Code, e.Message, e.Property));

        var response = ResponseDto<object>.FromValidation(
            ex.Error.Code,
            errors,
            context.TraceIdentifier);

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleApplication(
        HttpContext context,
        ApplicationExceptionBase ex)
    {
        var response = ResponseDto<object>.Fail(
                ex.Error.Code,
                ex.Error.Description)
            .WithCorrelationId(context.TraceIdentifier);

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleDomain(
        HttpContext context,
        DomainExceptionBase ex)
    {
        var response = ResponseDto<object>.Fail(
                ex.Error.Code,
                ex.Error.Description)
            .WithCorrelationId(context.TraceIdentifier);

        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleInfrastructure(
        HttpContext context,
        InfrastructureExceptionBase ex)
    {
        var response = ResponseDto<object>.Fail(
                ex.Error.Code,
                ex.Error.Description)
            .WithCorrelationId(context.TraceIdentifier);

        context.Response.StatusCode =
            ex.HttpStatusCode ?? 500;

        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleUnhandled(
        HttpContext context,
        Exception ex)
    {
        _logger.LogError(ex, "Unhandled error {TraceId}",
            context.TraceIdentifier);

        var response = ResponseDto<object>.Fail(
                "INTERNAL_SERVER_ERROR",
                "Error interno del servidor.")
            .WithCorrelationId(context.TraceIdentifier);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(response);
    }
}

