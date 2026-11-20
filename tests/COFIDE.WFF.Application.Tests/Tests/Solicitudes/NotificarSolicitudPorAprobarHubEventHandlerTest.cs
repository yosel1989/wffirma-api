using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class NotificarSolicitudPorAprobarHubEventHandlerTest
{
    private readonly Mock<ISolicitudQueryRepository> _consultas = new();
    private readonly Mock<INotificacionTiempoRealPort> _hub = new();

    private NotificarSolicitudPorAprobarHubEventHandler Handler() => new(_consultas.Object, _hub.Object);

    private static DomainEventNotification<SolicitudPorAprobarFirmantesDomainEvent> Evento(long id) =>
        new(new SolicitudPorAprobarFirmantesDomainEvent(id));

    [Fact]
    public async Task Handle_AvisaACadaFirmanteEnTurno()
    {
        using var cancelacion = new CancellationTokenSource();
        _consultas.Setup(x => x.ObtenerDestinatariosSolicitudPendienteFirmaAsync(5, cancelacion.Token))
            .ReturnsAsync(new DestinatarioSolicitudPendienteFirmaDto[] { new("jperez", 5), new("mlopez", 2) });

        await Handler().Handle(Evento(5), cancelacion.Token);

        _hub.Verify(x => x.NotificarSolicitudPendienteFirma(
            5, 5, It.Is<List<string>>(usuarios => usuarios.Count == 1 && usuarios[0] == "jperez"),
            "Tiene una solicitud pendiente por firmar o aprobar", cancelacion.Token), Times.Once);
        _hub.Verify(x => x.NotificarSolicitudPendienteFirma(
            5, 2, It.Is<List<string>>(usuarios => usuarios.Count == 1 && usuarios[0] == "mlopez"),
            "Tiene una solicitud pendiente por firmar o aprobar", cancelacion.Token), Times.Once);
        _hub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_SinFirmantesEnTurnoNoAvisa()
    {
        _consultas.Setup(x => x.ObtenerDestinatariosSolicitudPendienteFirmaAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DestinatarioSolicitudPendienteFirmaDto>());

        await Handler().Handle(Evento(5), default);

        _hub.VerifyNoOtherCalls();
    }
}
