using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Models;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

public sealed class SolicitudEscrituraAuthorizationTest
{
    [Fact]
    public void ValidarPermisoEscritura_DesdeAreaEspecial_DebeDenegar()
    {
        var solicitud = CrearSolicitud();
        var usuario = new Usuario(99, RolUsuario.Coordinador);

        var act = () => solicitud.ValidarPermisoEscritura(usuario, 70, [2]);

        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DOM_SOL_050");
    }

    [Fact]
    public void EditarSolicitudBorrador_DesdeAreaEspecial_DebeDenegarAntesDeModificar()
    {
        var solicitud = CrearSolicitud();
        var usuario = new Usuario(99, RolUsuario.Coordinador);
        var modelo = new SolicitudEditModel
        {
            Asunto = "No autorizado",
            NuevosDocumentosRequest = [],
            NuevosSustentosRequest = [],
            CodigosReferencia = [],
            Correos = []
        };

        var act = () => solicitud.EditarSolicitudBorrador(
            modelo, () => 1, () => 1, () => 1, () => 1, usuario, 70, [2]);

        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DOM_SOL_050");
        solicitud.Asunto.Should().Be("Contrato");
    }

    [Fact]
    public void ValidarPermisoEscritura_AlSolicitante_DebePermitirAunqueTengaAccesoEspecial()
    {
        var solicitud = CrearSolicitud();
        var usuario = new Usuario(10, RolUsuario.SolicitanteAprobador);

        var act = () => solicitud.ValidarPermisoEscritura(usuario, 70, [2]);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidarPermisoReuso_DesdeAreaEspecial_DebeDenegar()
    {
        var solicitud = CrearSolicitud();
        var usuario = new Usuario(99, RolUsuario.SolicitanteAprobador);

        var act = () => solicitud.ValidarPermisoReuso(usuario, 70, [5]);

        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DOM_SOL_050");
    }

    [Fact]
    public void Anular_DesdeAreaEspecial_DebeDenegarSinCambiarEstado()
    {
        var solicitud = CrearSolicitud();
        var usuario = new Usuario(99, RolUsuario.SolicitanteAprobador);

        var act = () => solicitud.Anular(usuario, 70, [2]);

        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DOM_SOL_050");
        solicitud.EstadoSolicitud.Should().Be(Domain.ValueObjects.EstadoSolicitud.Borrador);
    }

    private static Solicitud CrearSolicitud() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
