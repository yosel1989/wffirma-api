using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaWebHook;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
using COFIDE.WFF.Application.Features.Firmas.Commands.ValidarEstadoDetalleFirma;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]

public class FirmaController : BaseApiController
{
    public FirmaController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

    
    [HttpPost("solicitudes/{id}/firma/interna/pdf")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.FirmaController.FirmaInternaPdf.Summary,
        Description = DescriptionSwagger.FirmaController.FirmaInternaPdf.Description,
        OperationId = "FirmaInternaPdf"
        )]
    public async Task<ActionResult<FirmaInternaResponseDto>> FirmaInternaPdf([FromRoute] long id, [FromBody] FirmaInternaPdfRequest request)
    {
        var command = new FirmaInternaPdfCommand(id, request.idSolicitudDocumento);

        var result = await _mediator.Send(command);

        var response = ResponseDto<FirmaInternaResponseDto?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPost("solicitudes/{id}/firma/interna/zip")]
    [SwaggerOperation(
         Summary = DescriptionSwagger.FirmaController.FirmaInternaZip.Summary,
         Description = DescriptionSwagger.FirmaController.FirmaInternaZip.Description,
         OperationId = "FirmaInternaZip"
         )]
    public async Task<ActionResult<FirmaInternaResponseDto>> FirmaInternaZip([FromRoute] long id, [FromBody] FirmaInternaZipRequest request)
    {
        var command = new FirmaInternaZipCommand(id, request.Documentos);

        var result = await _mediator.Send(command);

        var response = ResponseDto<FirmaInternaResponseDto?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPost("solicitudes/firma/interna/resultado")]
    [SwaggerOperation(
         Summary = DescriptionSwagger.FirmaController.FirmaInternaResultado.Summary,
         Description = DescriptionSwagger.FirmaController.FirmaInternaResultado.Description,
         OperationId = "FirmaInternaResultado"
         )]
    public async Task<ActionResult<string>> FirmaInternaResultado(
    [FromBody] FirmaInternaWebHookCommand cmd)
    {
        var result = await _mediator.Send(cmd);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPost("solicitudes/{id}/firma/externa")]
    [SwaggerOperation(
         Summary = DescriptionSwagger.FirmaController.FirmaExterna.Summary,
         Description = DescriptionSwagger.FirmaController.FirmaExterna.Description,
         OperationId = "FirmaExterna"
         )]
    public async Task<ActionResult<string>> FirmaExterna([FromRoute] long id, [FromForm] FirmaExternaRequest request)
    {
        var command = new FirmaExternaCommand(
               id,
               request.IdSolicitudDocumento,
               request.Documento
           );

        var result = await _mediator.Send(command);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPost("solicitudes/{id}/firma/detalle/validaciones")]
    public async Task<ActionResult<ValidarEstadoDetalleFirmaDto>> ValidarDetalleFirma([FromRoute] long id, [FromBody] ValidarEstadoDetalleFirmaRequest request)
    {
        var command = new ValidarEstadoDetalleFirmaCommand(id, request.Detalles);

        var result = await _mediator.Send(command);

        var response = ResponseDto<ValidarEstadoDetalleFirmaDto?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    [HttpPost("solicitudes/firma/detalle/restablecer-firmados")]
    public async Task<ActionResult<string>> ReestablecerDocumentosFirmados([FromBody] RestablecerDocumentoFirmadoRequest request)
    {
        var command = new RestablecerDocumentoFirmadoCommand(request.idDetalleFirma);

        var result = await _mediator.Send(command);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

}
