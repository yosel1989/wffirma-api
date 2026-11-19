using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargosCombo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace COFIDE.WFF.Api.Controllers.v1;

/// <summary>Consultas del catálogo de cargos.</summary>
[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/cargos")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class CargoController : BaseApiController
{
    /// <summary>Inicializa el controlador de cargos.</summary>
    /// <param name="mediator">Mediador de las consultas.</param>
    /// <param name="correlationContext">Contexto de correlación de la solicitud.</param>
    public CargoController(IMediator mediator, ICorrelationContext correlationContext)
        : base(mediator, correlationContext) { }

    /// <summary>Obtiene los cargos disponibles para un combo, ordenados por nombre.</summary>
    /// <param name="cancellationToken">Token de cancelación de la solicitud.</param>
    /// <returns>Lista de identificadores y nombres de cargos.</returns>
    /// <response code="200">Se encontraron cargos.</response>
    /// <response code="404">No se encontraron cargos.</response>
    [HttpGet("combo")]
    [ProducesResponseType(typeof(ResponseDto<List<CargosComboDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<List<CargosComboDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDto<List<CargosComboDto>>>> ObtenerCombo(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerCargosComboQuery(), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<CargosComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<List<CargosComboDto>>.Ok(result));
    }
}
