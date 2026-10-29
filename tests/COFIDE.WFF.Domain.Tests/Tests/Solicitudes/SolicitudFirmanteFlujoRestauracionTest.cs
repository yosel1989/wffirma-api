using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

public class SolicitudFirmanteFlujoRestauracionTest
{
    private static readonly DateTime T1 = new(2026, 10, 29, 12, 07, 0);
    private static readonly DateTime T2 = new(2026, 10, 29, 12, 33, 0);
    private static readonly DateTime T3 = new(2026, 10, 29, 12, 42, 0);

    [Fact]
    public void ObtenerHistorialHasta_Debe_Devolver_El_Mas_Reciente_Anterior_O_Igual_Al_Punto()
    {
        var firmante = Firmante(1715, ordenEtapa: 1);
        firmante.AgregarHistorialRevision(79, true, T1, 1);
        firmante.AgregarHistorialRevision(84, true, T3, 1);

        var historial = firmante.ObtenerHistorialHasta(T2);

        historial.Should().NotBeNull();
        historial!.Id.Should().Be(79);
        historial.RevAprobado.Should().BeTrue();
    }

    [Fact]
    public void ObtenerHistorialHasta_Sin_Revisiones_Previas_Debe_Devolver_Nulo()
    {
        var firmante = Firmante(1720, ordenEtapa: 3);
        firmante.AgregarHistorialRevision(85, false, T3, 1);

        firmante.ObtenerHistorialHasta(T2).Should().BeNull();
    }

    [Fact]
    public void EsPosteriorA_Debe_Ser_Verdadero_Por_Fecha_O_Por_Etapa()
    {
        var punto = Firmante(1718, ordenEtapa: 2);
        punto.AgregarHistorialRevision(82, true, T2, 1);

        var anterior = Firmante(1715, ordenEtapa: 1);
        anterior.AgregarHistorialRevision(79, true, T1, 1);

        var posteriorPorFecha = Firmante(1719, ordenEtapa: 2);
        posteriorPorFecha.AgregarHistorialRevision(83, true, T3, 1);

        var posteriorPorEtapa = Firmante(1720, ordenEtapa: 3);

        anterior.EsPosteriorA(punto).Should().BeFalse();
        punto.EsPosteriorA(punto).Should().BeFalse();
        posteriorPorFecha.EsPosteriorA(punto).Should().BeTrue();
        posteriorPorEtapa.EsPosteriorA(punto).Should().BeTrue();
        punto.EsMismoOPosteriorA(punto).Should().BeTrue();
        anterior.EsMismoOPosteriorA(punto).Should().BeFalse();
    }

    [Fact]
    public void RestaurarAprobacion_Debe_Dejar_Al_Firmante_Aprobado_Con_Su_Fecha()
    {
        var firmante = Firmante(1715, ordenEtapa: 1);
        firmante.ResetearPorRechazo();

        firmante.RestaurarAprobacion(T1);

        firmante.RevAprobado.Should().BeTrue();
        firmante.FechaRevision.Should().Be(T1);
    }

    [Fact]
    public void InactivarHistorialRevisionPosterior_En_Rechazo_Debe_Conservar_La_Aprobacion_Del_Punto()
    {
        var firmante = Firmante(1718, ordenEtapa: 2);
        firmante.AgregarHistorialRevision(82, true, T2, 1);
        firmante.AgregarHistorialRevision(83, true, T3, 1);

        firmante.InactivarHistorialRevisionPosterior(T2);

        firmante.HistRevision.Single(h => h.Id == 82).Estado.Should().BeTrue();
        firmante.HistRevision.Single(h => h.Id == 83).Estado.Should().BeFalse();
    }

    [Fact]
    public void InactivarHistorialRevisionPosterior_Incluyendo_El_Punto_Debe_Inactivar_Desde_Esa_Fecha()
    {
        var firmante = Firmante(1718, ordenEtapa: 2);
        firmante.AgregarHistorialRevision(81, true, T1, 1);
        firmante.AgregarHistorialRevision(82, true, T2, 1);
        firmante.AgregarHistorialRevision(83, false, T3, 1);

        firmante.InactivarHistorialRevisionPosterior(T2, incluirPuntoSeleccionado: true);

        firmante.HistRevision.Single(h => h.Id == 81).Estado.Should().BeTrue();
        firmante.HistRevision.Single(h => h.Id == 82).Estado.Should().BeFalse();
        firmante.HistRevision.Single(h => h.Id == 83).Estado.Should().BeFalse();
    }

    [Fact]
    public void InactivarHistorialRevisionPosterior_Debe_Inactivar_El_Rechazo_Desde_El_Punto()
    {
        var firmante = Firmante(1720, ordenEtapa: 3);
        firmante.AgregarHistorialRevision(85, false, T2, 1);

        firmante.InactivarHistorialRevisionPosterior(T2);

        firmante.HistRevision.Single().Estado.Should().BeFalse();
    }

    private static SolicitudFirmanteFlujo Firmante(long id, long ordenEtapa) =>
        SolicitudFirmanteFlujo.Crear(id, idFirmanteFlujo: id, idSolicitudPlantilla: 824, idPlantillaEtapa: ordenEtapa, ordenEtapa, idUsuarioAsignado: 100 + id);
}
