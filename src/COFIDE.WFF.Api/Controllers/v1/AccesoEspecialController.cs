using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Mappings;
using COFIDE.WFF.Api.Request.AccesosEspeciales;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.EliminarAccesoEspecial;
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

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto<bool>>> EliminarAccesoEspecial(
    long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new EliminarAccesoEspecialCommand(id), cancellationToken);

        var response = ResponseDto<bool>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    
    [HttpPut("{id:long}/estado")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto<bool>>> CambiarEstadoAccesoEspecial(
    long id, CancellationToken cancellationToken)
    {
        // Devuelve el nuevo estado: true = activo, false = inactivo
        var nuevoEstado = await _mediator.Send(new CambiarEstadoAccesoEspecialCommand(id), cancellationToken);

        var response = ResponseDto<bool>.Ok(
            nuevoEstado,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

}
