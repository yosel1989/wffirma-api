using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Application.Common.Enums;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
namespace COFIDE.WFF.Api.Controllers.v1;

[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/public")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class PublicoController : BaseApiController
{
    private readonly IZipStreamingService _zipStreamingService;

    public PublicoController(IMediator mediator, ICorrelationContext correlationContext, IZipStreamingService zipStreamingService) : base(mediator, correlationContext)
    {
        _zipStreamingService = zipStreamingService;
    }

    [AllowAnonymous]
    [HttpGet("{token}")]
    public async Task<IActionResult> Descargar(
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(
                new DescargaDocumentoQuery(token), CancellationToken.None);

            if (result.Tipo == TipoDescarga.ArchivoUnico)
            {
                var archivo = result.Archivos.First();

                if (!System.IO.File.Exists(archivo.RutaFisica))
                    return NotFound(new { message = "Archivo no encontrado físicamente." });

                var stream = new FileStream(
                    archivo.RutaFisica,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                return File(
                    stream,
                    result.ContentType,
                    result.NombreArchivo);
            }

            Response.ContentType = result.ContentType;
            Response.Headers.ContentDisposition =
                $"attachment; filename=\"{result.NombreArchivo}\"";

            await _zipStreamingService.StreamZipAsync(
                Response.Body,
                result.Archivos,
                cancellationToken);

            return new EmptyResult();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
