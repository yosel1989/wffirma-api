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
using COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using COFIDE.WFF.Api.Request.Plantillas;
using COFIDE.WFF.Api.Mappings;
using COFIDE.WFF.Application.Common.Ports.Out;
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
    public async Task<ActionResult<long>> Create([FromBody] CrearPlantillaRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToCommand(), cancellationToken);

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
    public async Task<ActionResult<long>> Update([FromRoute] long id, [FromBody] ActualizarPlantillaRequest request, CancellationToken cancellationToken)
    {
        var cmd = request.ToCommand(id);

        var result = await _mediator.Send(cmd, cancellationToken);

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
    public async Task<ActionResult<ResponseDto<PagedResultDto<PlantillaPaginadoDto>>>> ObtenerPaginadoPlantilla([FromQuery] BuscarPlantillasRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<PagedResultDto<PlantillaPaginadoDto>>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>
    /// Gestión de plantillas: listado paginado con filtros de gerencia, departamento, creador, aprobador, nombre y fechas.
    /// </summary>
    [HttpGet("gestion-plantillas")]
    [ProducesResponseType(typeof(ResponseDto<GestionPlantillasResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<GestionPlantillasResponseDto>>> GestionPlantillasPaginado(
        [FromQuery] GestionPlantillasRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<GestionPlantillasResponseDto>.Ok(
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
    public async Task<ActionResult<ResponseDto<PlantillaDto>>> GetPlantillaPorId(long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerPlantillaPorIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound(ResponseDto<PlantillaDto>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<PlantillaDto>.Ok(result));
    }


    [HttpGet("validaciones")]
    [ProducesResponseType(typeof(ResponseDto<ValidarExistenciaResponseDto?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<ValidarExistenciaResponseDto?>>> ValidarNombrePlantilla([FromQuery] string texto, [FromQuery] bool visibilidad, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ValidarExistenciaQuery(texto, visibilidad), cancellationToken);
        return Ok(ResponseDto<ValidarExistenciaResponseDto?>.Ok(result));
    }


    #endregion
}
