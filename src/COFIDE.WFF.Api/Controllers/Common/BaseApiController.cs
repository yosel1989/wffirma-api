using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using COFIDE.WFF.Application.Common.Ports.Out;
namespace COFIDE.WFF.Api.Controllers.Common;

public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator _mediator;
    protected readonly ICorrelationContext _correlationContext;

    protected BaseApiController(IMediator mediator, ICorrelationContext correlationContext)
    {
        _mediator = mediator;
        _correlationContext = correlationContext;
    }

    /// <summary>
    /// Envía un request a MediatR y devuelve un ResponseDto con manejo automático de códigos de respuesta.
    /// </summary>
    protected async Task<ActionResult<ResponseDto<T>>> Send<T>(IRequest<ResponseDto<T>> request, CancellationToken cancellationToken = default)
    {
        var dto = await _mediator.Send(request, cancellationToken);

        // Aseguramos que el DTO tenga CorrelationId
        dto.CorrelationId ??= _correlationContext.CorrelationId;

        if (dto.Success)
            return Ok(dto);

        return dto.Code switch
        {
            ResponseCodes.NotFound => NotFoundResponse(dto),
            ResponseCodes.Unauthorized => UnauthorizedResponse(dto),
            ResponseCodes.Forbidden => ForbiddenResponse(dto),
            ResponseCodes.InternalError => InternalErrorResponse(dto),
            _ => Failure(dto)
        };
    }

    #region Métodos convenientes para respuestas explícitas

    protected ActionResult<ResponseDto<T>> Failure<T>(ResponseDto<T> dto)
        => BadRequest(dto);

    protected ActionResult<ResponseDto<T>> NotFoundResponse<T>(ResponseDto<T> dto)
        => NotFound(dto);

    protected ActionResult<ResponseDto<T>> UnauthorizedResponse<T>(ResponseDto<T> dto)
        => Unauthorized(dto);

    protected ActionResult<ResponseDto<T>> ForbiddenResponse<T>(ResponseDto<T> dto)
        => StatusCode(StatusCodes.Status403Forbidden, dto);

    protected ActionResult<ResponseDto<T>> InternalErrorResponse<T>(ResponseDto<T> dto)
        => StatusCode(StatusCodes.Status500InternalServerError, dto);

    #endregion
}