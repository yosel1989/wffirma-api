using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudEventoFactoryTest
{
    [Fact]
    public void CrearEventos_Sin_Cambios_Debe_Responder_Vacio()
    {
        var eventos = Factory().CrearEventos(8, new SolicitudFlujoDiff());

        eventos.Should().BeEmpty();
    }

    [Fact]
    public void CrearEventos_Debe_Registrar_Etapas_Y_Firmantes()
    {
        var diff = new SolicitudFlujoDiff
        {
            EtapasAgregadas = [new EtapaAgregada(1, 3, "Etapa", Guid.Empty)],
            EtapasEliminadas = [new EtapaEliminada(2, 4, "Vieja")],
            FirmantesAgregados = [new FirmanteAgregado(20, 3, Guid.Empty, "Etapa")],
            FirmantesEliminados = [new FirmanteEliminado(21, 4, "Vieja")]
        };

        var eventos = Factory().CrearEventos(8, diff);

        eventos.Select(e => e.IdTipoEvento).Should().Equal(5, 6, 3, 4);
        eventos.Should().OnlyContain(e => e.IdSolicitud == 8 && e.IdUsuarioOrigen == 10 && e.AreaUsuario == "GTI");
        eventos[2].IdUsuarioDestino.Should().Be(20);
        eventos[2].IdPlantillaEtapa.Should().Be(3);
    }

    [Fact]
    public void CrearEventoSustento_Debe_Registrar_El_Documento()
    {
        var evento = Factory().CrearEventoSustento(8, 6).Single();

        evento.IdTipoEvento.Should().Be(7);
        evento.IdSolicitud.Should().Be(8);
        evento.IdSolicitudSustento.Should().Be(6);
        evento.IdUsuarioOrigen.Should().Be(10);
        evento.AreaUsuario.Should().Be("GTI");
    }

    private static SolicitudEventoFactory Factory()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        return new SolicitudEventoFactory(usuario.Object);
    }
}
