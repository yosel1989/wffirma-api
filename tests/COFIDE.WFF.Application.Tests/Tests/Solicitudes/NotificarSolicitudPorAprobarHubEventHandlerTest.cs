using COFIDE.WFF.Application.Common.Constants;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class NotificarSolicitudPorAprobarHubEventHandlerTest
{
    private readonly Mock<ISolicitudQueryRepository> _consultas = new();
    private static readonly string EnCurso = COFIDE.WFF.Domain.ValueObjects.EstadoSolicitud.EnCurso.Codigo.ToString();

    private readonly Mock<INotificacionTiempoRealPort> _hub = new();

    private NotificarSolicitudPorAprobarHubEventHandler Handler() => new(_consultas.Object, _hub.Object);

    private static DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent> Evento(long id) =>
        new(new SolicitudPorAprobarFirmantesDomainEvent(id));

    [Fact]
    public async Task Handle_AvisaACadaFirmanteEnTurno()
    {
        using var cancelacion = new CancellationTokenSource();
        _consultas.Setup(x => x.ObtenerCodigosUsuarioEnTurnoAsync(5, cancelacion.Token))
            .ReturnsAsync(new[] { "jperez", "mlopez" });

        await Handler().Handle(Evento(5), cancelacion.Token);

        foreach (var codigo in new[] { "jperez", "mlopez" })
        {
            _hub.Verify(x => x.NotificarFirmaDocumentoAsync(
                5, 0, EnCurso, false, codigo, CodigosNotificacionTiempoReal.SolicitudPorAprobar, cancelacion.Token), Times.Once);
        }
        _hub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_SinFirmantesEnTurnoNoAvisa()
    {
        _consultas.Setup(x => x.ObtenerCodigosUsuarioEnTurnoAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        await Handler().Handle(Evento(5), default);

        _hub.VerifyNoOtherCalls();
    }
}
