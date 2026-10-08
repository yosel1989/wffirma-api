using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Dtos;
using COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using COFIDE.WFF.Application.Common.Ports.Out;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/usuarios")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class UsuarioController : BaseApiController
{
    public UsuarioController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

    #region Queries

    /// <summary>
    /// Obtener el contexto de mi Usuario
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ResponseDto<UsuarioContextoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioContextoDto>> ObtenerContextoUsuario(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerContextoUsuarioQuery(), cancellationToken);

        var response = ResponseDto<UsuarioContextoDto>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>Combo: Buecar Ususario por nombres-area-cargo</summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ResponseDto<List<UsuarioBuscadorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<List<UsuarioBuscadorDto>>>> BuscarUsuarios(
    [FromQuery] string texto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new BuscarUsuariosQuery(texto), cancellationToken);
        if (result.Count == 0)
            return NotFound(ResponseDto<List<UsuarioBuscadorDto>>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<List<UsuarioBuscadorDto>>.Ok(result));
    }

   
    #endregion
}
