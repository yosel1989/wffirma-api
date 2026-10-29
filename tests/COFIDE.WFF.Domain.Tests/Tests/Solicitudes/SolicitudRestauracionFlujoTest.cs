using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

public class SolicitudRestauracionFlujoTest
{
    [Fact]
    public void Restaurar_Rechazada_Debe_Reaprobar_A_Quienes_Ya_Habian_Aprobado_Y_Copiar_Sus_Firmas()
    {
        var flujo = RestauracionFlujoMother.RechazadaEnTercero();

        var rutas = flujo.Solicitud.RestaurarFlujo(flujo.Segundo.Id, idUsuarioAccion: 10, RestauracionFlujoMother.IdsDetalle());

        rutas.Should().BeEmpty();
        flujo.Solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
        flujo.Solicitud.MotivoRechazo.Should().BeNull();

        flujo.Primero.RevAprobado.Should().BeTrue();
        flujo.Primero.FechaRevision.Should().Be(flujo.T1);
        flujo.Segundo.RevAprobado.Should().BeTrue();
        flujo.Segundo.FechaRevision.Should().Be(flujo.T2);
        flujo.Tercero.RevAprobado.Should().BeFalse();
        flujo.Tercero.FechaRevision.Should().BeNull();

        DetalleActivo(flujo, flujo.Primero).TieneDocumentoFirmado().Should().BeTrue();
        DetalleActivo(flujo, flujo.Segundo).TieneDocumentoFirmado().Should().BeTrue();
        DetalleActivo(flujo, flujo.Tercero).TieneDocumentoFirmado().Should().BeFalse();

        flujo.Documento.DetallesFirma
            .Where(d => !d.Estado && d.TieneDocumentoFirmado())
            .Should().HaveCount(2);

        Historial(flujo.Primero, 10).Estado.Should().BeTrue();
        Historial(flujo.Segundo, 11).Estado.Should().BeTrue();
        Historial(flujo.Tercero, 12).Estado.Should().BeFalse();

        flujo.Solicitud.DomainEvents.Should().ContainSingle(e => e is SolicitudPorAprobarFirmantesDomainEvent);
        flujo.Solicitud.DomainEvents.Should().ContainSingle(e => e is SolicitudEstadoCambiadoDomainEvent);
    }

    [Fact]
    public void Restaurar_Aprobada_Debe_Reiniciar_Desde_El_Punto_Y_Devolver_Las_Rutas_Firmadas()
    {
        var flujo = RestauracionFlujoMother.Aprobada();

        var rutas = flujo.Solicitud.RestaurarFlujo(flujo.Segundo.Id, idUsuarioAccion: 10, RestauracionFlujoMother.IdsDetalle());

        flujo.Solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
        flujo.Primero.RevAprobado.Should().BeTrue();
        flujo.Primero.FechaRevision.Should().Be(flujo.T1);
        flujo.Segundo.RevAprobado.Should().BeFalse();
        flujo.Segundo.FechaRevision.Should().BeNull();
        flujo.Tercero.RevAprobado.Should().BeFalse();
        flujo.Tercero.FechaRevision.Should().BeNull();

        DetalleActivo(flujo, flujo.Primero).TieneDocumentoFirmado().Should().BeTrue();
        DetalleActivo(flujo, flujo.Segundo).TieneDocumentoFirmado().Should().BeFalse();
        DetalleActivo(flujo, flujo.Tercero).TieneDocumentoFirmado().Should().BeFalse();

        rutas.Should().BeEquivalentTo(
            ["/firmados/doc-101.pdf", "/firmados/doc-102.pdf"]);

        Historial(flujo.Primero, 10).Estado.Should().BeTrue();
        Historial(flujo.Segundo, 11).Estado.Should().BeFalse();
        Historial(flujo.Tercero, 12).Estado.Should().BeFalse();
    }

    [Fact]
    public void Restaurar_En_Curso_Debe_Reiniciar_Solo_A_Quienes_Ya_Intervinieron_Desde_El_Punto()
    {
        var flujo = RestauracionFlujoMother.EnCursoConDosAprobados();

        var rutas = flujo.Solicitud.RestaurarFlujo(flujo.Primero.Id, idUsuarioAccion: 10, RestauracionFlujoMother.IdsDetalle());

        flujo.Solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
        flujo.Firmantes.Should().OnlyContain(f => !f.RevAprobado && f.FechaRevision == null);
        flujo.Documento.DetallesFirma.Where(d => d.Estado).Should().HaveCount(3);
        flujo.Documento.DetallesFirma.Where(d => d.Estado).Should().OnlyContain(d => !d.TieneDocumentoFirmado());

        rutas.Should().BeEquivalentTo(
            ["/firmados/doc-100.pdf", "/firmados/doc-101.pdf"]);

        Historial(flujo.Primero, 10).Estado.Should().BeFalse();
        Historial(flujo.Segundo, 11).Estado.Should().BeFalse();
        flujo.Tercero.HistRevision.Should().BeEmpty();
        flujo.Solicitud.DomainEvents.Should().ContainSingle(e => e is SolicitudPorAprobarFirmantesDomainEvent);
        flujo.Solicitud.DomainEvents.Should().NotContain(e => e is SolicitudEstadoCambiadoDomainEvent);
    }

    [Fact]
    public void Restaurar_En_Curso_No_Debe_Tocar_A_Quienes_Van_Antes_Del_Punto()
    {
        var flujo = RestauracionFlujoMother.EnCursoConDosAprobados();

        flujo.Solicitud.RestaurarFlujo(flujo.Segundo.Id, idUsuarioAccion: 10, RestauracionFlujoMother.IdsDetalle());

        flujo.Primero.RevAprobado.Should().BeTrue();
        flujo.Primero.FechaRevision.Should().Be(flujo.T1);
        flujo.Segundo.RevAprobado.Should().BeFalse();
        flujo.Tercero.RevAprobado.Should().BeFalse();
        DetalleActivo(flujo, flujo.Primero).TieneDocumentoFirmado().Should().BeTrue();
        Historial(flujo.Primero, 10).Estado.Should().BeTrue();
        Historial(flujo.Segundo, 11).Estado.Should().BeFalse();
    }

    [Fact]
    public void Restaurar_Borrador_Debe_Lanzar_DOM_SOL_041()
    {
        var solicitud = SolicitudMother.Borrador();

        var act = () => solicitud.RestaurarFlujo(1, 10, RestauracionFlujoMother.IdsDetalle());

        act.Should().Throw<DomainException>().Which.Error.Code.Should().Be("DOM_SOL_041");
    }

    [Fact]
    public void Restaurar_Anulada_Debe_Lanzar_DOM_SOL_041()
    {
        var flujo = RestauracionFlujoMother.EnCursoSinRevisiones();
        flujo.Solicitud.Anular(10);

        var act = () => flujo.Solicitud.RestaurarFlujo(flujo.Primero.Id, 10, RestauracionFlujoMother.IdsDetalle());

        act.Should().Throw<DomainException>().Which.Error.Code.Should().Be("DOM_SOL_041");
    }

    [Fact]
    public void Restaurar_Sin_Flujo_Debe_Lanzar_DOM_SOL_006()
    {
        var solicitud = SolicitudMother.EnCurso();

        var act = () => solicitud.RestaurarFlujo(1, 10, RestauracionFlujoMother.IdsDetalle());

        act.Should().Throw<DomainException>().Which.Error.Code.Should().Be("DOM_SOL_006");
    }

    [Fact]
    public void Restaurar_Con_Punto_Inexistente_Debe_Lanzar_DOM_SOL_042()
    {
        var flujo = RestauracionFlujoMother.EnCursoConDosAprobados();

        var act = () => flujo.Solicitud.RestaurarFlujo(999, 10, RestauracionFlujoMother.IdsDetalle());

        act.Should().Throw<DomainException>().Which.Error.Code.Should().Be("DOM_SOL_042");
    }

    [Fact]
    public void Restaurar_Con_Punto_Sin_Fecha_Debe_Lanzar_DOM_SOL_043()
    {
        var flujo = RestauracionFlujoMother.EnCursoSinRevisiones();

        var act = () => flujo.Solicitud.RestaurarFlujo(flujo.Primero.Id, 10, RestauracionFlujoMother.IdsDetalle());

        act.Should().Throw<DomainException>().Which.Error.Code.Should().Be("DOM_SOL_043");
    }

    private static DetalleFirma DetalleActivo(FlujoRestauracion flujo, SolicitudFirmanteFlujo firmante) =>
        flujo.Documento.DetallesFirma.Single(d => d.Estado && d.IdSolicitudFirmanteFlujo == firmante.Id);

    private static HistorialRevision Historial(SolicitudFirmanteFlujo firmante, long id) =>
        firmante.HistRevision.Single(h => h.Id == id);
}
