using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Plantillas.Queries.PermisoEditarPlantilla;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.PermisoSolicitud;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CerrarSesion;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using COFIDE.WFF.Application.Common.Ports.Out;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class SeguridadController : BaseApiController
{
    public SeguridadController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

    [HttpGet("plantillas/{id}/permisos")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<bool>>> ValidarPermisoPlantilla(long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PermisoEditarPlantillaQuery(id), cancellationToken);
        return Ok(ResponseDto<bool>.Ok(result));
    }

    [HttpGet("solicitudes/{id}/permisos")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<bool>>> ValidarPermisoSolicitud(long id, bool revisar, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PermisoSolicitudQuery(id, revisar), cancellationToken);
        return Ok(ResponseDto<bool>.Ok(result));
    }

    [HttpPost("seguridad/cerrar-sesion")]
    [ProducesResponseType(typeof(ResponseDto<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<bool>>> CerrarSesion(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CerrarSesionCommand(), cancellationToken);
        var response = ResponseDto<bool>.Ok(
          result,
          "Operacion exitosa",
          HttpContext.TraceIdentifier);

        return Ok(response);
    }
}
