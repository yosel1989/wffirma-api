using System.Reflection;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Publico.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class CorreoPorAprobarEventHandlerTest
{
    [Fact]
    public async Task PorAprobar_Secuencial_Debe_Enviar_Un_Correo_Por_Firmante()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: false, "SECUENCIAL", "ana@cofide.com.pe", "luis@cofide.com.pe");

        await PorAprobar(solicitudes, plantillas, correos, formatos, renderer, urls);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "luis@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PorAprobar_Paralelo_Debe_Enviar_Un_Solo_Correo_Con_Todos_Los_Destinatarios()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: false, "PARALELO", "ana@cofide.com.pe", "luis@cofide.com.pe");

        await PorAprobar(solicitudes, plantillas, correos, formatos, renderer, urls);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe;luis@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PorAprobar_Urgente_No_Debe_Enviar_Con_La_Plantilla_Normal()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: true, "SECUENCIAL", "ana@cofide.com.pe");

        await PorAprobar(solicitudes, plantillas, correos, formatos, renderer, urls);

        correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Urgente_Cuando_La_Solicitud_No_Es_Urgente_No_Debe_Enviar()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: false, "SECUENCIAL", "ana@cofide.com.pe");

        await Urgente(solicitudes, plantillas, correos, formatos, renderer, urls);

        correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Urgente_Secuencial_Debe_Enviar_Un_Correo_Por_Firmante()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: true, "SECUENCIAL", true, "ana@cofide.com.pe", "luis@cofide.com.pe");

        await Urgente(solicitudes, plantillas, correos, formatos, renderer, urls);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "luis@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Urgente_Paralelo_Debe_Enviar_Un_Solo_Correo()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: true, "PARALELO", true, "ana@cofide.com.pe", "luis@cofide.com.pe");

        await Urgente(solicitudes, plantillas, correos, formatos, renderer, urls);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe;luis@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Urgente_Sin_Asunto_Debe_Tragar_La_Excepcion()
    {
        var (solicitudes, plantillas, correos, formatos, renderer, urls) =
            Armar(urgente: true, "SECUENCIAL", false, "ana@cofide.com.pe");

        var act = () => Urgente(solicitudes, plantillas, correos, formatos, renderer, urls);

        await act.Should().NotThrowAsync();
        correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Aprobado_Sin_Documentos_Debe_Enviar_Solo_Al_Solicitante()
    {
        var correos = new Mock<ICorreoSenderPort>();
        var enlaces = new Mock<IEnlaceDescargaTemporalRepository>();
        var uow = new Mock<IUnitOfWork>();

        await Aprobado(correos, enlaces, uow, tieneDocumento: false, conCorreosExtra: false, vigencia: []);

        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "otro@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Never);
        enlaces.Verify(e => e.AddAsync(It.IsAny<EnlaceDescargaTemporal>()), Times.Never);
    }

    [Fact]
    public async Task Aprobado_Con_Documentos_Debe_Crear_El_Enlace_Y_Avisar_A_Los_Otros_Correos()
    {
        var correos = new Mock<ICorreoSenderPort>();
        var enlaces = new Mock<IEnlaceDescargaTemporalRepository>();
        var uow = new Mock<IUnitOfWork>();

        await Aprobado(
            correos, enlaces, uow,
            tieneDocumento: true,
            conCorreosExtra: true,
            vigencia: [new ParametroDetalleDto { Id = 1, Codigo = "TI_VIGENCIA", Valor = "3" }]);

        enlaces.Verify(e => e.AddAsync(It.Is<EnlaceDescargaTemporal>(x => x.Token == "token-1")), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "ana@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        correos.Verify(c => c.EnviarCorreoAsync(
            "render", "render", "otro@cofide.com.pe", string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Task PorAprobar(
        Mock<ISolicitudRepository> solicitudes,
        Mock<ISolicitudPlantillaRepository> plantillas,
        Mock<ICorreoSenderPort> correos,
        Mock<IFormatoNotificacionRepository> formatos,
        Mock<IHtmlTemplateRenderer> renderer,
        Mock<IUrlProvider> urls) =>
        new EnviarCorreoPorAprobarHandler(
            solicitudes.Object, plantillas.Object, correos.Object, formatos.Object,
            renderer.Object, urls.Object, Logger<EnviarCorreoPorAprobarHandler>())
            .Handle(new DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent>(
                new SolicitudPorAprobarFirmantesDomainEvent(8)), CancellationToken.None);

    private static Task Urgente(
        Mock<ISolicitudRepository> solicitudes,
        Mock<ISolicitudPlantillaRepository> plantillas,
        Mock<ICorreoSenderPort> correos,
        Mock<IFormatoNotificacionRepository> formatos,
        Mock<IHtmlTemplateRenderer> renderer,
        Mock<IUrlProvider> urls) =>
        new EnviarCorreoUrgentePorAprobarHandler(
            solicitudes.Object, plantillas.Object, correos.Object, formatos.Object,
            renderer.Object, urls.Object, Logger<EnviarCorreoUrgentePorAprobarHandler>())
            .Handle(new DomainEventNotification<SolicitudCreadaDomainEvent>(
                new SolicitudCreadaDomainEvent(8, "ana@cofide.com.pe")), CancellationToken.None);

    private static Task Aprobado(
        Mock<ICorreoSenderPort> correos,
        Mock<IEnlaceDescargaTemporalRepository> enlaces,
        Mock<IUnitOfWork> uow,
        bool tieneDocumento,
        bool conCorreosExtra,
        List<ParametroDetalleDto> vigencia)
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        var formatos = new Mock<IFormatoNotificacionRepository>();
        var renderer = new Mock<IHtmlTemplateRenderer>();
        var urls = new Mock<IUrlProvider>();
        var tokens = new Mock<ITokenSeguroGenerator>();
        var parametros = new Mock<IParametroRepository>();

        urls.Setup(u => u.GetWorkflowBaseFrontUrl()).Returns("https://firmas.cofide");
        urls.Setup(u => u.GetWorkflowBaseBackUrl()).Returns("https://api.cofide/");
        formatos.Setup(f => f.ObtenerPorCodigo("SOLICITUD_APROBADA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormatoNotificacionPrueba());
        solicitudes.Setup(s => s.ObtenerParaNotificaciones(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Notificacion(false, tieneDocumento));

        var agregado = Borrador();
        if (conCorreosExtra)
            agregado.AgregarCorreo(1, "otro@cofide.com.pe");

        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(agregado);
        parametros.Setup(p => p.ObtenerCodigoParametroDias("TI_VIGENCIA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vigencia);
        tokens.Setup(t => t.Generar()).Returns("token-1");
        enlaces.Setup(e => e.AddAsync(It.IsAny<EnlaceDescargaTemporal>())).Returns(Task.CompletedTask);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        renderer.Setup(r => r.Render(It.IsAny<string>(), It.IsAny<object>())).Returns("render");
        correos.Setup(c => c.EnviarCorreoAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new EnviarCorreoAprobadoEventHandler(
            solicitudes.Object, correos.Object, formatos.Object, renderer.Object, urls.Object,
            enlaces.Object, tokens.Object, uow.Object, parametros.Object,
            Logger<EnviarCorreoAprobadoEventHandler>())
            .Handle(new DomainEventNotification<SolicitudAprobadaDomainEvent>(
                new SolicitudAprobadaDomainEvent(8, "Luis")), CancellationToken.None);
    }

    private static (
        Mock<ISolicitudRepository>,
        Mock<ISolicitudPlantillaRepository>,
        Mock<ICorreoSenderPort>,
        Mock<IFormatoNotificacionRepository>,
        Mock<IHtmlTemplateRenderer>,
        Mock<IUrlProvider>) Armar(bool urgente, string tipoEtapa, params string[] destinatarios) =>
        Armar(urgente, tipoEtapa, false, destinatarios);

    private static (
        Mock<ISolicitudRepository>,
        Mock<ISolicitudPlantillaRepository>,
        Mock<ICorreoSenderPort>,
        Mock<IFormatoNotificacionRepository>,
        Mock<IHtmlTemplateRenderer>,
        Mock<IUrlProvider>) Armar(bool urgente, string tipoEtapa, bool conAsunto, params string[] destinatarios)
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<ISolicitudPlantillaRepository>();
        var correos = new Mock<ICorreoSenderPort>();
        var formatos = new Mock<IFormatoNotificacionRepository>();
        var renderer = new Mock<IHtmlTemplateRenderer>();
        var urls = new Mock<IUrlProvider>();

        urls.Setup(u => u.GetWorkflowBaseFrontUrl()).Returns("https://firmas.cofide");
        var formato = new FormatoNotificacionPrueba();
        if (conAsunto)
        {
            Asignar(formato, nameof(FormatoNotificacion.AsuntoCorreo), "asunto");
            Asignar(formato, nameof(FormatoNotificacion.CuerpoMensaje), "cuerpo");
        }

        formatos.Setup(f => f.ObtenerPorCodigo(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(formato);
        solicitudes.Setup(s => s.ObtenerParaNotificaciones(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Notificacion(urgente));
        plantillas.Setup(p => p.ObtenerEtapaActual(8, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Etapa(tipoEtapa, destinatarios));
        renderer.Setup(r => r.Render(It.IsAny<string>(), It.IsAny<object>())).Returns("render");
        correos.Setup(c => c.EnviarCorreoAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return (solicitudes, plantillas, correos, formatos, renderer, urls);
    }

    private static PlantillaEtapaDto Etapa(string tipo, params string[] correos) => new()
    {
        NombreEtapa = "Etapa 1",
        ValorTipoEtapa = tipo,
        Firmantes = correos.Select((correo, i) => new FirmanteFlujoDto
        {
            Correo = correo,
            NombreUsuarioAsignado = $"Firmante {i + 1}"
        }).ToList()
    };

    private static FormatoNotificacionSolicitud Notificacion(bool urgente, bool tieneDocumento = false) =>
        new("Solicitud", "GTI-SOL-00000008-2026", "ana@cofide.com.pe", "Ana",
            "Contrato", "Alta", "Asunto", "2026-10-02", urgente, DateTime.Now.AddDays(2), null, null, tieneDocumento);

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);

    private static ILogger<T> Logger<T>() => new Mock<ILogger<T>>().Object;

    private static void Asignar(object destino, string propiedad, string valor)
    {
        var campo = typeof(FormatoNotificacion).GetField(
            $"<{propiedad}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        campo!.SetValue(destino, valor);
    }

    private sealed class FormatoNotificacionPrueba : FormatoNotificacion
    {
    }
}
