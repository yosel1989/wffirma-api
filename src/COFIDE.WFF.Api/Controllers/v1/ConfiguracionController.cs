using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitud;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacion;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoDocumentosCombo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Common.Constants;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class ConfiguracionController : BaseApiController
{
    public ConfiguracionController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

   
    [HttpGet("estadoDocumento/combo-estadoDocumento")]
    [ProducesResponseType(typeof(ResponseDto<List<EstadoDocumentosComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<EstadoDocumentosComboDto>>>> ObtenerCombosEstadoDocumentos(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerEstadoDocumentosComboQuery(), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<EstadoDocumentosComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<EstadoDocumentosComboDto>>.Ok(result));
    }

 

    /// <summary>Combo: Tipos de Solicitud filtrados por área y cargo del usuario</summary>
    [HttpGet("solicitudes/tipos")]
    [ProducesResponseType(typeof(ResponseDto<List<TipoSolicitudDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<TipoSolicitudDto>>>> GetTiposSolicitud(CancellationToken cancellationToken, [FromQuery] char estado = EstadoFiltro.Activo)
    {
        var result = await _mediator.Send(new ObtenerTipoSolicitudQuery(char.ToUpperInvariant(estado)), cancellationToken);

        if (result.Count == 0)
            return NotFound(ResponseDto<List<TipoSolicitudDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<List<TipoSolicitudDto>>.Ok(result));

    }

    /// <summary>Combo: Subtipos de Solicitud filtrados por tipo de solicitud</summary>
    [HttpGet("solicitudes/tipos/{idTipoSolicitud}/subtipos")]
    [ProducesResponseType(typeof(ResponseDto<List<SubTipoSolicitudDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<SubTipoSolicitudDto>>>> GetSubtiposSolicitud(long idTipoSolicitud, CancellationToken cancellationToken, [FromQuery] char estado = EstadoFiltro.Activo)
    {
        var result = await _mediator.Send(new ObtenerSubTipoSolicitudQuery(idTipoSolicitud, char.ToUpperInvariant(estado)), cancellationToken);

        if (result.Count == 0)
            return NotFound(ResponseDto<List<SubTipoSolicitudDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<List<SubTipoSolicitudDto>>.Ok(result));
    }

    /// <summary>Combo: Tipos de Operación filtrados por área y cargo del usuario</summary>
    [HttpGet("solicitudes/tipos-operacion")]
    [ProducesResponseType(typeof(ResponseDto<List<TipoOperacionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<TipoOperacionDto>>>> GetTipoOperacion(CancellationToken cancellationToken, [FromQuery] char estado = EstadoFiltro.Activo)
    {
        var result = await _mediator.Send(new ObtenerTipoOperacionQuery(char.ToUpperInvariant(estado)), cancellationToken);

        if (result.Count == 0)
            return NotFound(ResponseDto<List<TipoOperacionDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<List<TipoOperacionDto>>.Ok(result));
    }

}
