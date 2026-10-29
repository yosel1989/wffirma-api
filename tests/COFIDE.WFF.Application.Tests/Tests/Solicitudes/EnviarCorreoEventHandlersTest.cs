using System.Reflection;
using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Publico.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using COFIDE.WFF.Application.Tests.Support;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class EnviarCorreoEventHandlersTest
{
    private const long IdSolicitud = 8;

    [Fact]
    public async Task Borrador_Sin_Plantilla_No_Debe_Enviar_Ni_Propagar()
    {
        var fx = new CorreoFixture();
        fx.SinFormato(CodigosPlantillaCorreo.SolicitudBorrador);

        var act = () => fx.Borrador(new SolicitudCreadaBorradorDomainEvent(IdSolicitud, "ana@cofide.com.pe"));

        await act.Should().NotThrowAsync();
        fx.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Borrador_Sin_Solicitud_No_Debe_Enviar_Ni_Propagar()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudBorrador);
        fx.SinSolicitud();

        var act = () => fx.Borrador(new SolicitudCreadaBorradorDomainEvent(IdSolicitud, "ana@cofide.com.pe"));

        await act.Should().NotThrowAsync();
        fx.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Borrador_Debe_Enviar_Al_Correo_Del_Evento()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudBorrador);
        fx.ConSolicitud();

        await fx.Borrador(new SolicitudCreadaBorradorDomainEvent(IdSolicitud, "ana@cofide.com.pe"));

        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.Formatos.Verify(f => f.ObtenerPorCodigo(
            CodigosPlantillaCorreo.SolicitudBorrador, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Borrador_Sin_Correo_En_El_Evento_Debe_Usar_El_Destinatario_De_La_Plantilla()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudBorrador, destinatarioPlantilla: "copia@cofide.com.pe");
        fx.ConSolicitud();

        await fx.Borrador(new SolicitudCreadaBorradorDomainEvent(IdSolicitud, null));

        fx.EsperarEnvio("copia@cofide.com.pe");
    }

    [Fact]
    public async Task Creado_Sin_Plantilla_No_Debe_Enviar_Ni_Propagar()
    {
        var fx = new CorreoFixture();
        fx.SinFormato(CodigosPlantillaCorreo.SolicitudEnvio);

        var act = () => fx.Creado(new SolicitudCreadaDomainEvent(IdSolicitud, "ana@cofide.com.pe"));

        await act.Should().NotThrowAsync();
        fx.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Creado_Sin_Solicitud_No_Debe_Enviar_Ni_Propagar()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudEnvio);
        fx.SinSolicitud();

        var act = () => fx.Creado(new SolicitudCreadaDomainEvent(IdSolicitud, "ana@cofide.com.pe"));

        await act.Should().NotThrowAsync();
        fx.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Creado_Debe_Enviar_Al_Correo_Del_Evento()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudEnvio);
        fx.ConSolicitud();

        await fx.Creado(new SolicitudCreadaDomainEvent(IdSolicitud, "ana@cofide.com.pe"));

        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.Formatos.Verify(f => f.ObtenerPorCodigo(
            CodigosPlantillaCorreo.SolicitudEnvio, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PorAprobar_Cuando_Es_Urgente_No_Debe_Enviar()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobar);
        fx.ConSolicitud(urgente: true);
        fx.ConEtapa("SECUENCIAL", ("ana@cofide.com.pe", true));

        await fx.PorAprobar();

        fx.EsperarNingunEnvio();
    }

    [Fact]
    public async Task PorAprobar_Secuencial_Debe_Enviar_Un_Correo_Por_Firmante_Notificado()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobar);
        fx.ConSolicitud();
        fx.ConEtapa("SECUENCIAL", ("ana@cofide.com.pe", true), ("luis@cofide.com.pe", true), ("sin@cofide.com.pe", false));

        await fx.PorAprobar();

        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.EsperarEnvio("luis@cofide.com.pe");
        fx.EsperarEnvio("sin@cofide.com.pe", Times.Never());
    }

    [Fact]
    public async Task PorAprobar_Paralelo_Debe_Enviar_Un_Solo_Correo_Con_Los_Notificados()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobar);
        fx.ConSolicitud();
        fx.ConEtapa("PARALELO", ("ana@cofide.com.pe", true), ("luis@cofide.com.pe", true), ("sin@cofide.com.pe", null));

        await fx.PorAprobar();

        fx.EsperarEnvio("ana@cofide.com.pe;luis@cofide.com.pe");
        fx.Correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PorAprobar_Sin_Plantilla_O_Sin_Etapa_No_Debe_Enviar_Ni_Propagar()
    {
        var sinPlantilla = new CorreoFixture();
        sinPlantilla.SinFormato(CodigosPlantillaCorreo.SolicitudPorAprobar);

        var sinEtapa = new CorreoFixture();
        sinEtapa.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobar);
        sinEtapa.ConSolicitud();
        sinEtapa.SinEtapa();

        var actPlantilla = () => sinPlantilla.PorAprobar();
        var actEtapa = () => sinEtapa.PorAprobar();

        await actPlantilla.Should().NotThrowAsync();
        await actEtapa.Should().NotThrowAsync();
        sinPlantilla.EsperarNingunEnvio();
        sinEtapa.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Urgente_Cuando_La_Solicitud_No_Es_Urgente_No_Debe_Enviar()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobarUrgente, conCuerpo: true);
        fx.ConSolicitud(urgente: false);
        fx.ConEtapa("SECUENCIAL", ("ana@cofide.com.pe", true));

        await fx.Urgente();

        fx.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Urgente_Secuencial_Debe_Enviar_Un_Correo_Por_Firmante_Notificado()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobarUrgente, conCuerpo: true);
        fx.ConSolicitud(urgente: true);
        fx.ConEtapa("SECUENCIAL", ("ana@cofide.com.pe", true), ("luis@cofide.com.pe", true), ("sin@cofide.com.pe", false));

        await fx.Urgente();

        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.EsperarEnvio("luis@cofide.com.pe");
        fx.EsperarEnvio("sin@cofide.com.pe", Times.Never());
        fx.Formatos.Verify(f => f.ObtenerPorCodigo(
            CodigosPlantillaCorreo.SolicitudPorAprobarUrgente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Urgente_Paralelo_Debe_Enviar_Un_Solo_Correo_Con_Los_Notificados()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobarUrgente, conCuerpo: true);
        fx.ConSolicitud(urgente: true);
        fx.ConEtapa("PARALELO", ("ana@cofide.com.pe", true), ("luis@cofide.com.pe", true));

        await fx.Urgente();

        fx.EsperarEnvio("ana@cofide.com.pe;luis@cofide.com.pe");
    }

    [Fact]
    public async Task Urgente_Sin_Plantilla_O_Sin_Asunto_No_Debe_Enviar_Ni_Propagar()
    {
        var sinPlantilla = new CorreoFixture();
        sinPlantilla.SinFormato(CodigosPlantillaCorreo.SolicitudPorAprobarUrgente);
        sinPlantilla.ConSolicitud(urgente: true);

        var sinAsunto = new CorreoFixture();
        sinAsunto.ConFormato(CodigosPlantillaCorreo.SolicitudPorAprobarUrgente);
        sinAsunto.ConSolicitud(urgente: true);
        sinAsunto.ConEtapa("SECUENCIAL", ("ana@cofide.com.pe", true));

        var actPlantilla = () => sinPlantilla.Urgente();
        var actAsunto = () => sinAsunto.Urgente();

        await actPlantilla.Should().NotThrowAsync();
        await actAsunto.Should().NotThrowAsync();
        sinPlantilla.EsperarNingunEnvio();
        sinAsunto.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Rechazo_Sin_Plantilla_Solicitud_O_Etapa_No_Debe_Enviar_Ni_Propagar()
    {
        var sinPlantilla = new CorreoFixture();
        sinPlantilla.SinFormato(CodigosPlantillaCorreo.SolicitudRechazada);

        var sinSolicitud = new CorreoFixture();
        sinSolicitud.ConFormato(CodigosPlantillaCorreo.SolicitudRechazada);
        sinSolicitud.SinSolicitud();

        var sinEtapa = new CorreoFixture();
        sinEtapa.ConFormato(CodigosPlantillaCorreo.SolicitudRechazada);
        sinEtapa.ConSolicitud();
        sinEtapa.SinEtapa();

        await FluentActions.Awaiting(() => sinPlantilla.Rechazo()).Should().NotThrowAsync();
        await FluentActions.Awaiting(() => sinSolicitud.Rechazo()).Should().NotThrowAsync();
        await FluentActions.Awaiting(() => sinEtapa.Rechazo()).Should().NotThrowAsync();
        sinPlantilla.EsperarNingunEnvio();
        sinSolicitud.EsperarNingunEnvio();
        sinEtapa.EsperarNingunEnvio();
    }

    [Fact]
    public async Task Rechazo_Debe_Enviar_Al_Correo_Del_Solicitante()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudRechazada);
        fx.ConSolicitud();
        fx.ConEtapa("SECUENCIAL", ("ana@cofide.com.pe", true));

        await fx.Rechazo();

        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.Formatos.Verify(f => f.ObtenerPorCodigo(
            CodigosPlantillaCorreo.SolicitudRechazada, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Aprobado_Sin_Documentos_Debe_Enviar_Solo_Al_Solicitante()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudAprobada);
        fx.ConSolicitud(tieneDocumento: false);
        fx.ConAgregado();

        await fx.Aprobado();

        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.Correos.Verify(c => c.EnviarCorreoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        fx.Enlaces.Verify(e => e.ObtenerPorIdSolicitudAsync(IdSolicitud, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Aprobado_Con_Documentos_Debe_Leer_El_Enlace_Y_Avisar_A_Los_Otros_Correos()
    {
        var fx = new CorreoFixture();
        fx.ConFormato(CodigosPlantillaCorreo.SolicitudAprobada);
        fx.ConSolicitud(tieneDocumento: true);
        fx.ConAgregado("otro@cofide.com.pe");
        fx.ConEnlace();
        fx.Parametros.Setup(p => p.ObtenerCodigoParametroDias(
                CodigosParametro.VigenciaEnlaceDescarga, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = 1, Codigo = "TI_VIGENCIA", Valor = "3" }]);

        await fx.Aprobado();

        fx.Enlaces.Verify(e => e.ObtenerPorIdSolicitudAsync(IdSolicitud, It.IsAny<CancellationToken>()), Times.Once);
        fx.EsperarEnvio("ana@cofide.com.pe");
        fx.EsperarEnvio("otro@cofide.com.pe");
    }

    [Fact]
    public async Task Aprobado_Sin_Plantilla_O_Sin_Agregado_No_Debe_Enviar_Ni_Propagar()
    {
        var sinPlantilla = new CorreoFixture();
        sinPlantilla.SinFormato(CodigosPlantillaCorreo.SolicitudAprobada);

        var sinAgregado = new CorreoFixture();
        sinAgregado.ConFormato(CodigosPlantillaCorreo.SolicitudAprobada);
        sinAgregado.ConSolicitud();
        sinAgregado.SinAgregado();

        await FluentActions.Awaiting(() => sinPlantilla.Aprobado()).Should().NotThrowAsync();
        await FluentActions.Awaiting(() => sinAgregado.Aprobado()).Should().NotThrowAsync();
        sinPlantilla.EsperarNingunEnvio();
        sinAgregado.EsperarNingunEnvio();
    }

    private sealed class CorreoFixture
    {
        public Mock<ISolicitudQueryRepository> Solicitudes { get; } = new();
        public Mock<ISolicitudRepository> Agregados { get; } = new();
        public Mock<ISolicitudPlantillaRepository> Plantillas { get; } = new();
        public Mock<ICorreoSenderPort> Correos { get; } = new();
        public Mock<IFormatoNotificacionRepository> Formatos { get; } = new();
        public Mock<IHtmlTemplateRenderer> Renderer { get; } = new();
        public Mock<IUrlProvider> Urls { get; } = new();
        public Mock<IEnlaceDescargaTemporalRepository> Enlaces { get; } = new();
        public Mock<IParametroRepository> Parametros { get; } = new();

        public CorreoFixture()
        {
            Urls.Setup(u => u.GetWorkflowBaseFrontUrl()).Returns("https://firmas.cofide");
            Urls.Setup(u => u.GetWorkflowBaseBackUrl()).Returns("https://api.cofide/");
            Renderer.Setup(r => r.Render(It.IsAny<string>(), It.IsAny<object>())).Returns("render");
            Correos.Setup(c => c.EnviarCorreoAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            Parametros.Setup(p => p.ObtenerCodigoParametroDias(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
        }

        public void ConFormato(string codigo, bool conCuerpo = false, string? destinatarioPlantilla = null)
        {
            var formato = new FormatoNotificacionPrueba();
            if (conCuerpo)
            {
                Asignar(formato, nameof(FormatoNotificacion.AsuntoCorreo), "asunto");
                Asignar(formato, nameof(FormatoNotificacion.CuerpoMensaje), "cuerpo");
            }

            if (destinatarioPlantilla is not null)
                Asignar(formato, nameof(FormatoNotificacion.CorreoDestinatario), destinatarioPlantilla);

            Formatos.Setup(f => f.ObtenerPorCodigo(codigo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(formato);
        }

        public void SinFormato(string codigo) =>
            Formatos.Setup(f => f.ObtenerPorCodigo(codigo, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FormatoNotificacion?)null);

        public void ConSolicitud(bool urgente = false, bool tieneDocumento = false) =>
            Solicitudes.Setup(s => s.ObtenerParaNotificaciones(IdSolicitud, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FormatoNotificacionSolicitud(
                    "Solicitud", "GTI-SOL-00000008-2026", "ana@cofide.com.pe", "Ana",
                    "Contrato", "Alta", "Asunto", "2026-10-02", urgente, DateTime.Now.AddDays(2), null, null, tieneDocumento));

        public void SinSolicitud() =>
            Solicitudes.Setup(s => s.ObtenerParaNotificaciones(IdSolicitud, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FormatoNotificacionSolicitud?)null);

        public void ConEtapa(string tipo, params (string Correo, bool? Notificacion)[] firmantes) =>
            Plantillas.Setup(p => p.ObtenerEtapaActual(IdSolicitud, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PlantillaEtapaDto
                {
                    NombreEtapa = "Etapa 1",
                    ValorTipoEtapa = tipo,
                    Firmantes = firmantes.Select((firmante, indice) => new FirmanteFlujoDto
                    {
                        Correo = firmante.Correo,
                        NombreUsuarioAsignado = $"Firmante {indice + 1}",
                        Notificacion = firmante.Notificacion
                    }).ToList()
                });

        public void SinEtapa() =>
            Plantillas.Setup(p => p.ObtenerEtapaActual(IdSolicitud, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PlantillaEtapaDto?)null);

        public void ConAgregado(params string[] correosExtra)
        {
            var solicitud = Solicitud.CrearBorrador(
                IdSolicitud, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
                "Contrato", null, false, false, null, 0);
            for (var i = 0; i < correosExtra.Length; i++)
                solicitud.AgregarCorreo(i + 1, correosExtra[i]);

            Agregados.Setup(a => a.ObtenerAgregadoSolicitudAsync(
                    IdSolicitud, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                .ReturnsAsync(solicitud);
        }

        public void SinAgregado() =>
            Agregados.Setup(a => a.ObtenerAgregadoSolicitudAsync(
                    IdSolicitud, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                .ReturnsAsync((Solicitud?)null);

        public void ConEnlace() =>
            Enlaces.Setup(e => e.ObtenerPorIdSolicitudAsync(IdSolicitud, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EnlaceDescargaTemporal.Crear("token-1", IdSolicitud, DateTime.UtcNow.AddDays(3)));

        public Task Borrador(SolicitudCreadaBorradorDomainEvent evento) =>
            new EnviarCorreoBorradorEventHandler(
                Solicitudes.Object, Correos.Object, Formatos.Object, Renderer.Object, Urls.Object, ProtectorSolicitudPrueba.Crear(), Logger<EnviarCorreoBorradorEventHandler>())
                .Handle(new DomainEventNotification<SolicitudCreadaBorradorDomainEvent>(evento), CancellationToken.None);

        public Task Creado(SolicitudCreadaDomainEvent evento) =>
            new EnviarCorreoCreadoEventHandler(
                Solicitudes.Object, Correos.Object, Formatos.Object, Renderer.Object, Urls.Object, ProtectorSolicitudPrueba.Crear(), Logger<EnviarCorreoCreadoEventHandler>())
                .Handle(new DomainEventNotification<SolicitudCreadaDomainEvent>(evento), CancellationToken.None);

        public Task PorAprobar() =>
            new EnviarCorreoPorAprobarHandler(
                Solicitudes.Object, Plantillas.Object, Correos.Object, Formatos.Object, Renderer.Object, Urls.Object,
                ProtectorSolicitudPrueba.Crear(), Logger<EnviarCorreoPorAprobarHandler>())
                .Handle(new DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent>(
                    new SolicitudPorAprobarFirmantesDomainEvent(IdSolicitud)), CancellationToken.None);

        public Task Urgente() =>
            new EnviarCorreoUrgentePorAprobarHandler(
                Solicitudes.Object, Plantillas.Object, Correos.Object, Formatos.Object, Renderer.Object, Urls.Object,
                ProtectorSolicitudPrueba.Crear(), Logger<EnviarCorreoUrgentePorAprobarHandler>())
                .Handle(new DomainEventNotification<SolicitudCreadaDomainEvent>(
                    new SolicitudCreadaDomainEvent(IdSolicitud, "ana@cofide.com.pe")), CancellationToken.None);

        public Task Rechazo() =>
            new EnviarCorreoRechazoEventHandler(
                Solicitudes.Object, Plantillas.Object, Correos.Object, Formatos.Object, Renderer.Object, Urls.Object,
                ProtectorSolicitudPrueba.Crear(), Logger<EnviarCorreoRechazoEventHandler>())
                .Handle(new DomainEventNotification<SolicitudRechazadaDomainEvent>(
                    new SolicitudRechazadaDomainEvent(IdSolicitud, "Luis")), CancellationToken.None);

        public Task Aprobado() =>
            new EnviarCorreoAprobadoEventHandler(
                Agregados.Object, Solicitudes.Object, Correos.Object, Formatos.Object, Renderer.Object, Urls.Object,
                Enlaces.Object, Parametros.Object, ProtectorSolicitudPrueba.Crear(), Logger<EnviarCorreoAprobadoEventHandler>())
                .Handle(new DomainEventNotification<SolicitudAprobadaDomainEvent>(
                    new SolicitudAprobadaDomainEvent(IdSolicitud, "Luis")), CancellationToken.None);

        public void EsperarEnvio(string destinatario, Times? veces = null) =>
            Correos.Verify(c => c.EnviarCorreoAsync(
                "render", "render", destinatario, string.Empty, It.IsAny<CancellationToken>()),
                veces ?? Times.Once());

        public void EsperarNingunEnvio() =>
            Correos.Verify(c => c.EnviarCorreoAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }

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
