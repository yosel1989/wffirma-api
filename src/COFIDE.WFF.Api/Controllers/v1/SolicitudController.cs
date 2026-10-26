using Asp.Versioning;
using COFIDE.WFF.Api.ApiVersions;
using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Api.Controllers.Common;
using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Mappings;
using COFIDE.WFF.Api.Request.Solicitudes;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ActivarDocumentoBloqueado;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AnularSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumentoZip;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustento;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustentoZip;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarTodoSolicitudZip;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DetectarConflictosCodigosReferencia;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.GenerarPdfSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.PermisoSolicitud;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Security.Cryptography;
namespace COFIDE.WFF.Api.Controllers.v1;


[ApiController]
[ApiVersion(Version1.Version)]
[Route("api/v{version:apiVersion}/solicitudes")]
[Produces(MediaTypeNames.Application.Json)]
[ApiExplorerSettings(GroupName = Version1.GroupName)]
public class SolicitudController : BaseApiController
{
    public SolicitudController(IMediator mediator, ICorrelationContext correlationContext) : base(mediator, correlationContext) { }

    #region CREACIÓN DE SOLICITUD

    [HttpPost]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.Create.Summary,
        Description = DescriptionSwagger.SolicitudController.Create.Description,
        OperationId = "Create"
        )]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Create([FromForm] CrearSolicitudRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToRegistrarCommand(), cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    [HttpPost("envios")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.CreateEnviar.Summary,
        Description = DescriptionSwagger.SolicitudController.CreateEnviar.Description,
        OperationId = "CreateEnviar"
        )]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> CreateEnviar([FromForm] CrearSolicitudRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToEnviarCommand(), cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    [HttpPost("{id}/reusos")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.CrearSolicitudPorReuso.Summary,
        Description = DescriptionSwagger.SolicitudController.CrearSolicitudPorReuso.Description,
        OperationId = "CrearSolicitudPorReuso"
        )]
    public async Task<ActionResult<string>> CrearSolicitudPorReuso([FromRoute] long id, [FromForm] SolicitudFormRequest request, CancellationToken cancellationToken)
    {
        var cmd = new ReusarSolicitudCommand(id, request.ToSolicitudRequestDto());

        var result = await _mediator.Send(cmd, cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPost("{id}/reusos/envios")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.CrearSolicitudPorReusoYEnviar.Summary,
        Description = DescriptionSwagger.SolicitudController.CrearSolicitudPorReusoYEnviar.Description,
        OperationId = "CrearSolicitudPorReusoYEnviar"
        )]
    public async Task<ActionResult<string>> CrearSolicitudPorReusoYEnviar([FromRoute] long id, [FromForm] SolicitudFormRequest request, CancellationToken cancellationToken)
    {
        var cmd = new ReusarEnviarSolicitudCommand(id, request.ToSolicitudRequestDto());

        var result = await _mediator.Send(cmd, cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    #endregion

    #region EDICIÓN DE SOLICITUD

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.EditarSolicitud.Summary,
        Description = DescriptionSwagger.SolicitudController.EditarSolicitud.Description,
        OperationId = "EditarSolicitud"
        )]
    public async Task<ActionResult<string>> EditarSolicitud([FromRoute] long id, [FromForm] SolicitudFormRequest request, CancellationToken cancellationToken)
    {
        var cmd = new EditarSolicitudCommand(id, request.ToSolicitudRequestDto());

        var result = await _mediator.Send(cmd, cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPut("{id}/envios")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.EditarBorradorYFinalizar.Summary,
        Description = DescriptionSwagger.SolicitudController.EditarBorradorYFinalizar.Description,
        OperationId = "EditarBorradorYFinalizar"
        )]
    public async Task<ActionResult<string>> EditarBorradorYFinalizar([FromRoute] long id, [FromForm] SolicitudFormRequest request, CancellationToken cancellationToken)
    {
        var cmd = new EditarEnviarSolicitudCommand(id, request.ToSolicitudRequestDto());

        var result = await _mediator.Send(cmd, cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpPut("{id}/anulaciones")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.AnularSolicitud.Summary,
        Description = DescriptionSwagger.SolicitudController.AnularSolicitud.Description,
        OperationId = "AnularSolicitud"
        )]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Anular([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AnularSolicitudCommand(id), cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
           ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    #endregion

    #region REVISIONES

    [HttpPut("{id}/aprobaciones")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.AprobarSolicitud.Summary,
        Description = DescriptionSwagger.SolicitudController.AprobarSolicitud.Description,
        OperationId = "AprobarSolicitud"
        )]
    public async Task<ActionResult<string>> AprobarSolicitud([FromRoute] long id, [FromForm] AprobarSolicitudRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToCommand(id), cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    [HttpPut("{id}/rechazos")]
    [SwaggerOperation(
        Summary = DescriptionSwagger.SolicitudController.RechazarSolicitud.Summary,
        Description = DescriptionSwagger.SolicitudController.RechazarSolicitud.Description,
        OperationId = "RechazarSolicitud"
        )]
    public async Task<ActionResult<string>> RechazarSolicitud([FromRoute] long id, [FromForm] RechazarSolicitudRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToCommand(id), cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    #endregion




    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseDto<SolicitudDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<SolicitudDto>>> GetSolicitud(long id, [FromQuery] bool esReuso = false, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ObtenerSolicitudQuery(id, esReuso), cancellationToken);
        if (result is null)
            return NotFound(ResponseDto<SolicitudDto>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<SolicitudDto>.Ok(result));
    }




    #region LISTA-BANDEJA-PAGINACION

    [HttpGet("paginado")]
    [ProducesResponseType(typeof(SolicitudPaginadoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseDto<SolicitudPaginadoResponseDto>>> ObtenerSolicitudPaginado(
    [FromQuery] SolicitudPaginadoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<SolicitudPaginadoResponseDto>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>listado de solicitudes por firmar del usuario</summary>
    [HttpGet("bandeja-entrada")]
    [ProducesResponseType(typeof(ResponseDto<BandejaEntradaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<BandejaEntradaResponseDto>>> BandejaEntradaPaginado(
    [FromQuery] BandejaEntradaRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<BandejaEntradaResponseDto>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>listado de documentos bloqueado</summary>
    [HttpGet("documentos/bloqueados/paginado")]
    [ProducesResponseType(typeof(ResponseDto<PagedResultDto<DocumentoBloqueadoPaginadoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<PagedResultDto<DocumentoBloqueadoPaginadoDto>>>> GetListadoDocumentosBloqueados(
    [FromQuery] DocumentoBloqueadoPaginadoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<PagedResultDto<DocumentoBloqueadoPaginadoDto>>.Ok(
         result,
         "Operación exitosa",
         HttpContext.TraceIdentifier);

        return Ok(response);
    }
    #endregion



    [HttpGet("panel-control")]
    [ProducesResponseType(typeof(ResponseDto<SolicitudPanelControlDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<SolicitudPanelControlDto>>> GetSolicitudPanelControl(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObtenerSolicitudPanelControlQuery(), cancellationToken);
        if (result is null)
            return NotFound(ResponseDto<SolicitudPanelControlDto>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<SolicitudPanelControlDto>.Ok(result));
    }








    [HttpPost("codigos-referencia/conflictos")]
    [ProducesResponseType(typeof(ResponseDto<DetectarConflictosResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<DetectarConflictosResponse>>> GetDetectarcodigoReferencia(
        [FromBody] DetectarCodigosReferenciaRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        return Ok(ResponseDto<DetectarConflictosResponse>.Ok(result));
    }


    [HttpPost("documentos/firma/validaciones")]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>>>> ValidarDocumentoFirmado([FromForm] ValidarDocumentoFirmadoRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        var response = ResponseDto<IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>>.Ok(
          result,
          ResponseCodes.OperacionExitosa,
          HttpContext.TraceIdentifier);

        return Ok(response);
    }

    /// <summary>zip de documentos de solicitud</summary>
    [HttpGet("{id}/documentos/zip")]
    public async Task<IActionResult> DescargarDocumentosZip(long id, [FromQuery] List<long>? documentos, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DescargarSolicitudDocumentoZipQuery(id, documentos),
            cancellationToken);

        return File(result.Archivo, result.ContentType, result.NombreArchivo);
    }

   

   [HttpGet("{id}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarPdf(long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GenerarPdfSolicitudQuery(id), cancellationToken);

        if (result is null)
            return NotFound(ResponseDto<string>.Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));

        return File(result.Archivo, result.ContentType, result.NombreArchivo);
    }

    [HttpGet("{id}/descargar-zip")]
    public async Task<IActionResult> DescargarZip(long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DescargarTodoSolicitudZipQuery(id),
            cancellationToken);

        return File(result.Archivo, result.ContentType, result.NombreArchivo);
    }





    #region Documentos y Sustentos

    /// <summary>Obtener areas para combo</summary>
    [HttpGet("documentos/{id}/descarga")]
    [ProducesResponseType(typeof(ResponseDto<DescargarSolicitudDocumentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<DescargarSolicitudDocumentoDto>>> ObtenerDocumento(long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DescargarSolicitudDocumentoQuery(id), cancellationToken);
        if (result is null)
            return NotFound(ResponseDto<DescargarSolicitudDocumentoDto>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<DescargarSolicitudDocumentoDto>.Ok(result));
    }
    /// <summary>
    /// anular solicitud
    /// </summary>
    [HttpPut("documentos/{id}/activaciones")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> ActivarDocumento([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivarDocumentoBloqueadoCommand(id), cancellationToken);

        var response = ResponseDto<string?>.Ok(
            result,
            ResponseCodes.OperacionExitosa,
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    #endregion

    #region Solicitud Sustento

    [HttpGet("sustentos/{id}/descarga")]
    [ProducesResponseType(typeof(ResponseDto<DescargarSolicitudSustentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<DescargarSolicitudSustentoDto>>> ObtenerSustento(long id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DescargarSolicitudSustentoQuery(id), cancellationToken);
        if (result is null)
            return NotFound(ResponseDto<DescargarSolicitudSustentoDto>
                .Fail(ResponseCodes.NotFound, ResponseCodes.RegistrosNoEncontrados));
        return Ok(ResponseDto<DescargarSolicitudSustentoDto>.Ok(result));
    }

    /// <summary>zip de sustentos de solicitud</summary>
    [HttpGet("{id}/sustentos/zip")]
    public async Task<IActionResult> DescargarSustentosZip(long id, [FromQuery] List<long>? sustentos, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DescargarSolicitudSustentoZipQuery(id, sustentos),
            cancellationToken);

        return File(result.Archivo, result.ContentType, result.NombreArchivo);
    }

    #endregion

    #region Reporte

    /// <summary>Reporte de descarga de solicitudes</summary>
    [HttpGet("reporte")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReporteSolicitud(
        [FromQuery] ReporteSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.ToQuery(), cancellationToken);

        return File(
            result.Contenido,
            result.ContentType,
            result.NombreArchivo
        );
    }

    #endregion


    [HttpGet("seguimiento-solicitudes")]
    [ProducesResponseType(typeof(ResponseDto<SeguimientoSolicitudesResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<SeguimientoSolicitudesResponseDto>>> SeguimientoSolicitudesPaginado(
    [FromQuery] SeguimientoSolicitudesRequest request)
    {
        var result = await _mediator.Send(request.ToQuery());

        var response = ResponseDto<SeguimientoSolicitudesResponseDto>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }


    [HttpGet("gestion-solicitudes")]
    [ProducesResponseType(typeof(ResponseDto<ObtenerGestionSolicitudesResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseDto<ObtenerGestionSolicitudesResponseDto>>> GestionSolicitudesPaginado(
    [FromQuery] GestionSolicitudesRequest request)
    {
        var result = await _mediator.Send(request.ToQuery());

        var response = ResponseDto<ObtenerGestionSolicitudesResponseDto>.Ok(
            result,
            "Operación exitosa",
            HttpContext.TraceIdentifier);

        return Ok(response);
    }

    #region Desenscriptar id

    [HttpPost("desencriptar-id")]
    [ProducesResponseType(
        typeof(ResponseDto<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DesencriptarIdSolicitud(
        [FromBody] DesencriptarIdSolicitudRequest request,
        [FromServices] ISolicitudIdProtector solicitudIdProtector,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new
            {
                success = false,
                message = "El enlace de la solicitud no es válido."
            });
        }

        long idSolicitud;

        try
        {
            // Recuperar el ID original desde el token protegido.
            idSolicitud = solicitudIdProtector.Desencriptar(request.Token);
        }
        catch (CryptographicException)
        {
            return BadRequest(new
            {
                success = false,
                message = "El enlace es inválido o ha expirado."
            });
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new
            {
                success = false,
                message = "El enlace de la solicitud no es válido."
            });
        }

        var tienePermiso = await _mediator.Send(
            new PermisoSolicitudQuery(idSolicitud, request.Revisar),
            cancellationToken
        );

        if (!tienePermiso)
        {
            return Forbid();
        }

        var response = ResponseDto<object>.Ok(
            new { idSolicitud }
        );

        return Ok(response);
    }

    #endregion
}

