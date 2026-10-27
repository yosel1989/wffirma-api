using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Mappings;
using COFIDE.WFF.Api.Request.AccesosEspeciales;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/accesos-especiales")]
[Produces(MediaTypeNames.Application.Json)]
public class AccesoEspecialController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccesoEspecialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("paginado")]
    [ProducesResponseType(typeof(AccesoEspecialPaginadoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseDto<AccesoEspecialPaginadoResponseDto>>> ObtenerAccesosEspecialesPaginado(
        [FromQuery] AccesoEspecialPaginadoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<AccesoEspecialPaginadoResponseDto>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> RegistrarAccesoEspecial(
        [FromBody] RegistrarAccesoEspecialRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send( request.ToRegistrarCommand(), cancellationToken);
        var response = ResponseDto<long>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /*[HttpPut("estado")]
    public async Task<ActionResult<bool>> CambiarEstadoAccesoEspecial(
        [FromBody] CambiarEstadoAccesoEspecialRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CambiarEstadoAccesoEspecialCommand(request), cancellationToken);
        return Ok(result);
    }*/
}
