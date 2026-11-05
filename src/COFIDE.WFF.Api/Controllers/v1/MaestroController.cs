using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerDepartamentosCombo;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerListadoParametro;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoSolicitudCombo;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using COFIDE.WFF.Application.Common.Ports.Out;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/maestros")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class MaestroController : BaseApiController
{
    public MaestroController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

    [HttpGet("areas")]
    [ProducesResponseType(typeof(ResponseDto<List<AreasComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<AreasComboDto>>>> ObtenerCombosAreas(
      CancellationToken cancellationToken,
      [FromQuery] bool? excluirAreaUsuario,
      [FromQuery] char? tipo = 'G')
    {
        var result = await _mediator.Send(new ObtenerAreasComboQuery(tipo, excluirAreaUsuario), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<AreasComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<AreasComboDto>>.Ok(result));
    }

    [HttpGet("departamentos")]
    [ProducesResponseType(typeof(ResponseDto<List<DepartamentosComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<DepartamentosComboDto>>>> ObtenerCombosDepartamentos(
       [FromQuery] List<long> codigoGerencia, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerDepartamentosComboQuery(codigoGerencia), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<DepartamentosComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<DepartamentosComboDto>>.Ok(result));
    }

    [HttpGet("usuarios")]
    [ProducesResponseType(typeof(ResponseDto<List<UsuariosComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<UsuariosComboDto>>>> ObtenerComboUsuarios(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerUsuariosComboQuery(), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<UsuariosComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<UsuariosComboDto>>.Ok(result));
    }

    [HttpGet("tipos-solicitud")]
    [ProducesResponseType(typeof(ResponseDto<List<TipoSolicitudComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<TipoSolicitudComboDto>>>> ObtenerCombosTipoSolicitud([FromQuery] List<long>? idArea, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerTipoSolicitudPorAreaQuery(idArea), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<TipoSolicitudComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<TipoSolicitudComboDto>>.Ok(result));
    }

    [HttpGet("subtipos-solicitud")]
    [ProducesResponseType(typeof(ResponseDto<List<SubTipoSolicitudComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<SubTipoSolicitudComboDto>>>> ObtenerCombosTipoSolicitud(
       [FromQuery] List<long>? idArea,
       [FromQuery] List<long>? idTipoSolicitud, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerSubTipoSolicitudPorAreaQuery(idArea, idTipoSolicitud), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<SubTipoSolicitudComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<SubTipoSolicitudComboDto>>.Ok(result));
    }

    [HttpGet("tipos-operacion")]
    [ProducesResponseType(typeof(ResponseDto<List<TipoOperacionComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<TipoOperacionComboDto>>>> ObtenerCombosTipoOperacion([FromQuery] List<long>? idArea, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerTipoOperacionPorAreaQuery(idArea), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<TipoOperacionComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<TipoOperacionComboDto>>.Ok(result));
    }

    [HttpGet("estados-solicitud")]
    [ProducesResponseType(typeof(ResponseDto<List<EstadoSolicitudComboDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<EstadoSolicitudComboDto>>>> ObtenerCombosEstadoSolicitud(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerEstadoSolicitudComboQuery(), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<EstadoSolicitudComboDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<EstadoSolicitudComboDto>>.Ok(result));
    }

    [HttpGet("parametro")]
    [ProducesResponseType(typeof(ResponseDto<List<ListadoParametroDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<ListadoParametroDto>>>> GetParametroSolicitud(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerListadoParametroQuery(), cancellationToken);

        if (result.Count == 0)
            return NotFound(ResponseDto<List<ListadoParametroDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return Ok(ResponseDto<List<ListadoParametroDto>>.Ok(result));
    }
}
