using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Publico.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
namespace COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;

public class EnviarCorreoAprobadoEventHandler(
        ISolicitudRepository solicitudRepository,
        ICorreoSenderPort correoSenderPort,
        IFormatoNotificacionRepository formatoNotificacionRepository,
        IHtmlTemplateRenderer htmlTemplateRenderer,
        IUrlProvider urlProvider,
        IEnlaceDescargaTemporalRepository enlaceDescargaTemporalRepository,
        ITokenSeguroGenerator tokenSeguroGenerator,
        IUnitOfWork unitOfWork,
        IParametroRepository parametroRepository,
        ILogger<EnviarCorreoAprobadoEventHandler> logger
    ) : INotificationHandler<DomainEventNotification<SolicitudAprobadaDomainEvent>>
{
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ICorreoSenderPort _correoSenderPort = correoSenderPort;
    private readonly IFormatoNotificacionRepository _formatoNotificacionRepository = formatoNotificacionRepository;
    private readonly IHtmlTemplateRenderer _htmlTemplateRenderer = htmlTemplateRenderer;
    private readonly IUrlProvider _urlProvider = urlProvider;
    private readonly IEnlaceDescargaTemporalRepository _enlaceDescargaTemporalRepository = enlaceDescargaTemporalRepository;
    private readonly ITokenSeguroGenerator _tokenSeguroGenerator = tokenSeguroGenerator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly ILogger<EnviarCorreoAprobadoEventHandler> _logger = logger;

    public async Task Handle(DomainEventNotification<SolicitudAprobadaDomainEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var baseFrontUrl = _urlProvider.GetWorkflowBaseFrontUrl();
            var baseBackUrl = _urlProvider.GetWorkflowBaseBackUrl();
            var domainEvent = notification.DomainEvent;
            string correoUsuario = "";

            var templateCorreo = await _formatoNotificacionRepository
                .ObtenerPorCodigo("SOLICITUD_APROBADA", cancellationToken)
                ?? throw new InvalidOperationException("Template SOLICITUD_APROBADA no configurado.");

            var solicitud = await _solicitudRepository
                .ObtenerParaNotificaciones(domainEvent.IdSolicitud, cancellationToken)
                ?? throw new InvalidOperationException("Solicitud no encontrada para notificaciones.");

            var solicitudCorreos = await _solicitudRepository
                .ObtenerAgregadoSolicitudAsync(domainEvent.IdSolicitud, cancellationToken)
                ?? throw new InvalidOperationException("Agregado de solicitud no encontrado.");

            var parametroVigencia = await _parametroRepository
                .ObtenerCodigoParametroDias("TI_VIGENCIA", cancellationToken);

            var vigenciaDias = int.TryParse(
                parametroVigencia.FirstOrDefault()?.Valor, out var dias) ? dias : 1;

            string LinkDescargaDocumentos = string.Empty;
            var correos = solicitudCorreos.Correos;

            if (solicitud.TieneDocumento)
            {
                string tokenGenerado = _tokenSeguroGenerator.Generar();
                var enlaceDocumento = EnlaceDescargaTemporal.Crear(
                    tokenGenerado,
                    domainEvent.IdSolicitud,
                    DateTime.Now.AddDays(vigenciaDias));

                await _enlaceDescargaTemporalRepository.AddAsync(enlaceDocumento);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var linkDescarga = $"{baseBackUrl.TrimEnd('/')}/API-WFF/api/v1/public/{tokenGenerado}";

                LinkDescargaDocumentos =
                    $"Si desea descargue los documentos, estarán habilitados por {vigenciaDias} día(s): " +
                    $"<a href='{linkDescarga}' style='color:#004F91; text-decoration:underline;'>Descarga</a>" +
                    "<br/>";
            }

            var linkSolicitudHtml =
                $"<a href='{baseFrontUrl}/mis_solicitudes/detalle/visualizar?id={domainEvent.IdSolicitud}'" +
                " style='color:#004F91; text-decoration:underline;'>Ver solicitud</a>";

            var asunto = _htmlTemplateRenderer.Render(
                templateCorreo.AsuntoCorreo ?? string.Empty, solicitud);

            var modeloSolicitante = new
            {
                solicitud.TipoSolicitud,
                solicitud.CodigoSolicitud,
                solicitud.NombreSolicitante,
                solicitud.SubTipoSolicitud,
                solicitud.TipoOperacion,
                solicitud.AsuntoSolicitud,
                solicitud.FechaCreacion,
                UltimoAprobador = domainEvent.nombreAprobador,
                Anio = DateTime.Now.Year.ToString(),
                FechaAprobacionFinal = DateTime.Now.ToString("dd/MM/yyyy"),
                TiempoLimite = $"{vigenciaDias} día(s)",
                LinkDescargaDocumentos,
                LinkSolicitudHtml = linkSolicitudHtml
            };

            var cuerpoSolicitante = _htmlTemplateRenderer.Render(
                templateCorreo.CuerpoMensaje ?? string.Empty, modeloSolicitante);

            await _correoSenderPort.EnviarCorreoAsync(
                asunto,
                cuerpoSolicitante,
                solicitud.correoSolicitante ?? string.Empty,
                templateCorreo.CorreoRemitente ?? string.Empty,
                cancellationToken);

            var otrosDestinatarios = string.Join(";",
                new[] { correoUsuario }
                .Concat(correos?.Select(x => x.Correo.Value) ?? Enumerable.Empty<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
            );

            if (!string.IsNullOrWhiteSpace(otrosDestinatarios))
            {
                var modeloOtros = new
                {
                    solicitud.TipoSolicitud,
                    solicitud.CodigoSolicitud,
                    solicitud.NombreSolicitante,
                    solicitud.SubTipoSolicitud,
                    solicitud.TipoOperacion,
                    solicitud.AsuntoSolicitud,
                    solicitud.FechaCreacion,
                    UltimoAprobador = domainEvent.nombreAprobador,
                    Anio = DateTime.Now.Year.ToString(),
                    FechaAprobacionFinal = DateTime.Now.ToString("dd/MM/yyyy"),
                    TiempoLimite = $"{vigenciaDias} día(s)",
                    LinkDescargaDocumentos,
                    LinkSolicitudHtml = string.Empty
                };

                var cuerpoOtros = _htmlTemplateRenderer.Render(
                    templateCorreo.CuerpoMensaje ?? string.Empty, modeloOtros);

                await _correoSenderPort.EnviarCorreoAsync(
                    asunto,
                    cuerpoOtros,
                    otrosDestinatarios,
                    templateCorreo.CorreoRemitente ?? string.Empty,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar un correo borrador");
        }
    }
}