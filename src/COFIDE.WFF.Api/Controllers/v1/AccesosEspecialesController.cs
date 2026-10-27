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
public class AccesosEspecialesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccesosEspecialesController(IMediator mediator)
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

    /*[HttpPost]
    public async Task<ActionResult<long>> CrearAccesoEspecial(
        [FromBody] CrearAccesoEspecialRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CrearAccesoEspecialCommand(request), cancellationToken);
        return Ok(result);
    }*/

    /*[HttpPut("estado")]
    public async Task<ActionResult<bool>> CambiarEstadoAccesoEspecial(
        [FromBody] CambiarEstadoAccesoEspecialRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CambiarEstadoAccesoEspecialCommand(request), cancellationToken);
        return Ok(result);
    }*/
}
