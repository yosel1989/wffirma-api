using COFIDE.WFF.Domain.DomainServices;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;

public class ReglaPlantillaSolicitudTest
{
    [Theory]
    [InlineData(0L, null, false, false, AccionPlantillaSolicitud.SinPlantilla)]
    [InlineData(0L, 0L, false, true, AccionPlantillaSolicitud.CrearDesdeConfiguracion)]
    [InlineData(0L, 5L, true, true, AccionPlantillaSolicitud.CrearDesdeConfiguracion)]
    [InlineData(0L, 5L, true, false, AccionPlantillaSolicitud.CrearDesdeMaestra)]
    [InlineData(3L, 5L, false, true, AccionPlantillaSolicitud.ActualizarPlantillaOrigen)]
    [InlineData(3L, 5L, true, false, AccionPlantillaSolicitud.ActualizarPlantillaOrigen)]
    [InlineData(5L, 5L, false, true, AccionPlantillaSolicitud.ActualizarPlantillaSeleccionada)]
    [InlineData(5L, 5L, false, false, AccionPlantillaSolicitud.ActualizarPlantillaSeleccionada)]
    public void Decidir_Debe_Elegir_La_Accion_Segun_La_Plantilla_Y_El_Flujo(
        long? origen, long? seleccionada, bool esMaestra, bool tieneFlujo, AccionPlantillaSolicitud esperada)
    {
        ReglaPlantillaSolicitud.Decidir(origen, seleccionada, esMaestra, tieneFlujo).Should().Be(esperada);
    }

    [Fact]
    public void Una_Plantilla_Maestra_Nunca_Se_Actualiza_Directamente_En_Una_Solicitud_Nueva()
    {
        var accionConFlujo = ReglaPlantillaSolicitud.Decidir(0, 5, seleccionadaEsMaestra: true, tieneFlujoConfigurado: true);
        var accionSinFlujo = ReglaPlantillaSolicitud.Decidir(0, 5, seleccionadaEsMaestra: true, tieneFlujoConfigurado: false);

        accionConFlujo.Should().NotBe(AccionPlantillaSolicitud.ActualizarPlantillaSeleccionada);
        accionSinFlujo.Should().NotBe(AccionPlantillaSolicitud.ActualizarPlantillaSeleccionada);
    }
}
