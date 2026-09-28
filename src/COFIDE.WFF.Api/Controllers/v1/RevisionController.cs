using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;
using COFIDE.WFF.Application.Features.Revisiones.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/solicitudes")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]

public class RevisionController : BaseApiController
{
    public RevisionController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }



    #region FLUJOS
    [HttpPut("{id}/plantilla/{idPlantilla}/etapas")]
    public async Task<ActionResult<EditarFlujoResponseDto>> EditarFlujo([FromRoute] long id, [FromRoute] long idPlantilla, [FromBody] EditarFlujoRequest request)
    {
        var command = new EditarFlujoCommand(
             id,
             idPlantilla,
             request.Etapas
         );

        var result = await _mediator.Send(command);

        var response = ResponseDto<EditarFlujoResponseDto>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }
    #endregion  
}
