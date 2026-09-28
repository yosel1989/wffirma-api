using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Models;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/plantillas")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class PlantillaController : BaseApiController
{
    public PlantillaController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

    #region Commands

    /// <summary>
    /// Registrar una plantilla
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> Create([FromBody] RegistrarPlantillaCommand cmd)
    {
        var result = await _mediator.Send(cmd);

        var response = ResponseDto<long>.Ok(
            result,
            "Operacion exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>
    /// Editar una plantilla
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> Update([FromRoute] long id, [FromBody] EditarPlantillaRequest request)
    {
        var cmd = new EditarPlantillaCommand(
               id,
               request.NombrePlantilla,
               request.Descripcion,
               request.Etapas
        );

        var result = await _mediator.Send(cmd);

        var response = ResponseDto<long>.Ok(
            result,
            "Operacion exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    #endregion


    #region Queries

    /// <summary>
    /// Obtiene roles paginados con filtros y ordenamiento
    /// </summary>
    [HttpGet("paginado")]
    [ProducesResponseType(typeof(PagedResultDto<PlantillaPaginadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseDto<PagedResultDto<PlantillaPaginadoDto>>>> ObtenerPaginadoPlantilla(
     [FromQuery] ObtenerPlantillaPaginadoQuery query)
    {
        var result = await _mediator.Send(query);

        var response = ResponseDto<PagedResultDto<PlantillaPaginadoDto>>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>
    /// Obtener la plantilla, etapas y flujos
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseDto<PlantillaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<PlantillaDto>>> GetPlantillaPorId(long id)
    {
        var result = await _mediator.Send(new ObtenerPlantillaPorIdQuery(id));

        if (result == null)
            return NotFound(ResponseDto<PlantillaDto>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<PlantillaDto>.Ok(result));
    }


    [HttpGet("validaciones")]
    [ProducesResponseType(typeof(ResponseDto<ValidarExistenciaResponseDto?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<ValidarExistenciaResponseDto?>>> ValidarNombrePlantilla([FromQuery] string texto, [FromQuery] bool visibilidad)
    {
        var result = await _mediator.Send(new ValidarExistenciaQuery(texto, visibilidad));
        return Ok(ResponseDto<ValidarExistenciaResponseDto?>.Ok(result));
    }


    #endregion
}
