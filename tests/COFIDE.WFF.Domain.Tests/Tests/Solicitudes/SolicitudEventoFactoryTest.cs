using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Factories;
using COFIDE.WFF.Domain.DomainServices;
using FluentAssertions;
using VO = COFIDE.WFF.Domain.ValueObjects;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

/// <summary>A2: la fábrica de eventos de historial ahora es del dominio y no depende de la sesión.</summary>
public class SolicitudEventoFactoryTests
{
    [Fact]
    public void Cambio_De_Flujo_Debe_Generar_Un_Evento_Por_Cada_Cambio_Con_Su_Tipo()
    {
        var diff = new SolicitudFlujoDiff();
        diff.EtapasAgregadas.Add(new EtapaAgregada(3, 30, "Nueva", Guid.NewGuid()));
        diff.EtapasEliminadas.Add(new EtapaEliminada(2, 20, "Quitada"));
        diff.FirmantesAgregados.Add(new FirmanteAgregado(200, 10, Guid.Empty, "Etapa 1"));
        diff.FirmantesEliminados.Add(new FirmanteEliminado(100, 10, "Etapa 1"));

        var eventos = SolicitudEventoFactory.CrearEventosCambioFlujo(8, diff, idUsuario: 5, areaUsuario: "GTI");

        eventos.Should().HaveCount(4);
        eventos.Should().OnlyContain(e => e.IdSolicitud == 8 && e.IdUsuarioOrigen == 5 && e.AreaUsuario == "GTI");
        eventos.Select(e => e.IdTipoEvento).Should().Equal(
            VO.TipoEvento.FlowStageAdded.Codigo,
            VO.TipoEvento.FlowStageRemoved.Codigo,
            VO.TipoEvento.FlowApproverAdded.Codigo,
            VO.TipoEvento.FlowApproverRemoved.Codigo);
        eventos.Single(e => e.IdTipoEvento == VO.TipoEvento.FlowApproverAdded.Codigo)
            .IdUsuarioDestino.Should().Be(200);
    }

    [Fact]
    public void Sin_Cambios_No_Debe_Generar_Eventos()
    {
        SolicitudEventoFactory.CrearEventosCambioFlujo(8, new SolicitudFlujoDiff(), 5, "GTI")
            .Should().BeEmpty();
    }

    [Fact]
    public void Sustento_Debe_Generar_Un_Evento_DocumentAdded_Con_El_Sustento()
    {
        var eventos = SolicitudEventoFactory.CrearEventoSustento(8, idSustento: 77, idUsuario: 5, areaUsuario: "GTI");

        eventos.Should().ContainSingle().Which.Should().Match<SolicitudEventoRegistradoDomainEvent>(e =>
            e.IdTipoEvento == VO.TipoEvento.DocumentAdded.Codigo
            && e.IdSolicitudSustento == 77
            && e.IdUsuarioOrigen == 5);
    }
}
