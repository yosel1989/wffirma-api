using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using COFIDE.WFF.Domain.ValueObjects;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

public class SolicitudVisualizacionTest
{
    [Fact]
    public void RegistrarVista_SoloActualizaLaFechaYConservaPrimeraFecha()
    {
        var flujo = RestauracionFlujoMother.EnCursoSinRevisiones();
        using (HoraSistema.Fijar(flujo.T1))
            Assert.Equal(flujo.T1, flujo.Primero.RegistrarVistaSolicitud());
        using (HoraSistema.Fijar(flujo.T2))
            Assert.Equal(flujo.T1, flujo.Primero.RegistrarVistaSolicitud());

        Assert.Equal(flujo.T1, flujo.Primero.FechaVistaSolicitud);
        Assert.Null(flujo.Segundo.FechaVistaSolicitud);
        Assert.Null(flujo.Tercero.FechaVistaSolicitud);
        Assert.False(flujo.Primero.RevAprobado);
        Assert.Null(flujo.Primero.FechaRevision);
        Assert.Empty(flujo.Primero.HistRevision);
        Assert.Empty(flujo.Solicitud.DomainEvents);
    }

    [Fact]
    public void Rechazar_LimpiaVistasDeTodosLosFirmantes()
    {
        var flujo = RestauracionFlujoMother.EnCursoSinRevisiones();
        foreach (var firmante in flujo.Firmantes)
            firmante.RegistrarVistaSolicitud();

        flujo.Solicitud.RechazarSolicitud(flujo.UsuarioPrimero, "Aprobador", "Corregir documento",
            flujo.Primero.IdPlantillaEtapa, "GTI", 100, 1);

        Assert.Equal(EstadoSolicitud.Rechazado, flujo.Solicitud.EstadoSolicitud);
        Assert.All(flujo.Firmantes, f => Assert.Null(f.FechaVistaSolicitud));
    }

    [Fact]
    public void Restaurar_LimpiaVistaDelPendienteQueYaAbrioElDetalle()
    {
        var flujo = RestauracionFlujoMother.EnCursoConDosAprobados();
        flujo.Primero.RegistrarVistaSolicitud();
        flujo.Segundo.RegistrarVistaSolicitud();
        flujo.Tercero.RegistrarVistaSolicitud();

        flujo.Solicitud.RestaurarFlujo(flujo.Segundo.Id, 10, RestauracionFlujoMother.IdsDetalle());

        Assert.NotNull(flujo.Primero.FechaVistaSolicitud);
        Assert.Null(flujo.Segundo.FechaVistaSolicitud);
        Assert.Null(flujo.Tercero.FechaVistaSolicitud);
        Assert.True(flujo.Primero.RevAprobado);
        Assert.False(flujo.Segundo.RevAprobado);
        Assert.False(flujo.Tercero.RevAprobado);
    }

    [Fact]
    public void Resetear_PermiteRegistrarOtraFechaSinCambiarLaFechaDeRevision()
    {
        var firmante = SolicitudFirmanteFlujo.Crear(1, 2, 3, 4, 1, 100);
        using (HoraSistema.Fijar(new DateTime(2026, 11, 17, 10, 0, 0)))
            firmante.RegistrarVistaSolicitud();
        firmante.ResetearPorRechazo();
        var nuevaFecha = new DateTime(2026, 11, 18, 11, 30, 0);
        using (HoraSistema.Fijar(nuevaFecha))
            Assert.Equal(nuevaFecha, firmante.RegistrarVistaSolicitud());

        Assert.Equal(nuevaFecha, firmante.FechaVistaSolicitud);
        Assert.Null(firmante.FechaRevision);
    }
}
