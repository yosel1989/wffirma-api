using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;

namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;

public class PlantillaEstadoTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CambiarEstado_ConPlantillaEliminada_LanzaError_RF06(bool activar)
    {
        var plantilla = PlantillaMother.Vacia();
        plantilla.EliminarLogicamente();

        var exception = Assert.Throws<DomainException>(() =>
        {
            if (activar)
                plantilla.Activar();
            else
                plantilla.Desactivar();
        });

        Assert.Equal(PlantillaErrors.PlantillaEliminada, exception.Error);
        Assert.Equal(EstadoPlantilla.Eliminada, plantilla.EstadoPlantilla);
        Assert.False(plantilla.Estado);
    }

    [Fact]
    public void CambiarEstado_ConPlantillaNoEliminada_MantieneComportamientoIdempotente_RF06()
    {
        var plantilla = PlantillaMother.Vacia();

        plantilla.Activar();
        Assert.Equal(EstadoPlantilla.Activa, plantilla.EstadoPlantilla);
        Assert.True(plantilla.Estado);

        plantilla.Desactivar();
        plantilla.Desactivar();
        Assert.Equal(EstadoPlantilla.Inactiva, plantilla.EstadoPlantilla);
        Assert.False(plantilla.Estado);

        plantilla.Activar();
        plantilla.Activar();
        Assert.Equal(EstadoPlantilla.Activa, plantilla.EstadoPlantilla);
        Assert.True(plantilla.Estado);
    }

    [Fact]
    public void PermitirCambiarEstado_ConCoordinadorDeAreaPermite_RF02()
    {
        var plantilla = PlantillaMother.Vacia();
        AsignarArea(plantilla, 100, 200);

        var permitido = plantilla.PermitirCambiarEstadoPlantilla(
            2,
            100,
            200,
            new Usuario(2, RolUsuario.Coordinador),
            []);

        Assert.True(permitido);
    }

    [Fact]
    public void PermitirCambiarEstado_ConCoordinadorDeAccesoEspecialPermite_RF02()
    {
        var plantilla = PlantillaMother.Vacia();
        AsignarArea(plantilla, 100, 200);

        var permitido = plantilla.PermitirCambiarEstadoPlantilla(
            2,
            300,
            400,
            new Usuario(2, RolUsuario.Coordinador),
            [100]);

        Assert.True(permitido);
    }

    [Fact]
    public void PermitirCambiarEstado_ConCoordinadorFueraDeAlcanceDeniega_RF02()
    {
        var plantilla = PlantillaMother.Vacia();
        AsignarArea(plantilla, 100, 200);

        var permitido = plantilla.PermitirCambiarEstadoPlantilla(
            2,
            300,
            400,
            new Usuario(2, RolUsuario.Coordinador),
            []);

        Assert.False(permitido);
    }

    [Fact]
    public void PermitirCambiarEstado_ConAdministradorPermite_RF02()
    {
        var plantilla = PlantillaMother.Vacia();

        var permitido = plantilla.PermitirCambiarEstadoPlantilla(
            1,
            null,
            null,
            new Usuario(1, RolUsuario.Administrador),
            []);

        Assert.True(permitido);
    }

    [Fact]
    public void PermitirCambiarEstado_ConSolicitantePropietarioPermite_RF02()
    {
        var plantilla = Plantilla.Crear(
            1, "Plantilla", "Descripción", true, 10, "GTI", true);

        var permitido = plantilla.PermitirCambiarEstadoPlantilla(
            10,
            null,
            null,
            new Usuario(10, RolUsuario.SolicitanteAprobador),
            []);

        Assert.True(permitido);
    }

    [Fact]
    public void PermitirCambiarEstado_ConSolicitanteQueNoEsPropietarioDeniega_RF02()
    {
        var plantilla = Plantilla.Crear(
            1, "Plantilla", "Descripción", true, 10, "GTI", true);

        var exception = Assert.Throws<DomainException>(() =>
            plantilla.PermitirCambiarEstadoPlantilla(
                11,
                null,
                null,
                new Usuario(11, RolUsuario.SolicitanteAprobador),
                []));

        Assert.Equal(PlantillaErrors.PropietarioPlantilla, exception.Error);
    }

    private static void AsignarArea(Plantilla plantilla, long area, long areaPadre)
    {
        typeof(Plantilla).GetProperty(nameof(Plantilla.IdAreaPlantilla))!
            .SetValue(plantilla, area);
        typeof(Plantilla).GetProperty(nameof(Plantilla.IdAreaPadrePlantilla))!
            .SetValue(plantilla, areaPadre);
    }
}
