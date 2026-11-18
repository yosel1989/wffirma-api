using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Exceptions;
using Moq;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class PlantillaCambioEstadoAuthorizationServiceTest
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IAccesoEspecialRepository> _accesoEspecial = new();

    [Fact]
    public async Task AutorizarAsync_ConAdministrador_Permite_RF02()
    {
        ConfigurarUsuario(1, RolUsuario.Administrador, RolUsuarioCodigo.Administrador);

        await CrearServicio().AutorizarAsync(
            CrearPlantilla(), CancellationToken.None);

        _accesoEspecial.Verify(
            repository => repository.GetAreasByUserIdAsync(It.IsAny<long>()),
            Times.Never);
    }

    [Fact]
    public async Task AutorizarAsync_ConCoordinadorDeAreaYGerenica_Permite_RF02()
    {
        ConfigurarUsuario(2, RolUsuario.Coordinador, RolUsuarioCodigo.Coordinador,
            100, 200);
        var plantilla = CrearPlantilla();
        AsignarArea(plantilla, 100, 200);

        await CrearServicio().AutorizarAsync(plantilla, CancellationToken.None);
    }

    [Fact]
    public async Task AutorizarAsync_ConCoordinadorDeAccesoEspecial_Permite_RF02()
    {
        ConfigurarUsuario(3, RolUsuario.Coordinador, RolUsuarioCodigo.Coordinador,
            300, 400);
        var plantilla = CrearPlantilla();
        AsignarArea(plantilla, 100, 200);
        _accesoEspecial.Setup(repository => repository.GetAreasByUserIdAsync(3))
            .ReturnsAsync([100]);

        await CrearServicio().AutorizarAsync(plantilla, CancellationToken.None);
    }

    [Fact]
    public async Task AutorizarAsync_ConPropietarioSolicitante_Permite_RF02()
    {
        ConfigurarUsuario(10, RolUsuario.SolicitanteAprobador,
            RolUsuarioCodigo.SolicitanteAprobador);

        await CrearServicio().AutorizarAsync(
            CrearPlantilla(), CancellationToken.None);
    }

    [Fact]
    public async Task AutorizarAsync_ConSolicitanteNoPropietario_Deniega_RF02()
    {
        ConfigurarUsuario(11, RolUsuario.SolicitanteAprobador,
            RolUsuarioCodigo.SolicitanteAprobador);

        var act = () => CrearServicio().AutorizarAsync(
            CrearPlantilla(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<DomainException>(act);
        Assert.Equal("DOM_PLT_027", exception.Error.Code);
    }

    [Fact]
    public async Task AutorizarAsync_ConCoordinadorSinAlcance_Deniega_RF02()
    {
        ConfigurarUsuario(4, RolUsuario.Coordinador, RolUsuarioCodigo.Coordinador,
            300, 400);
        var plantilla = CrearPlantilla();
        AsignarArea(plantilla, 100, 200);
        _accesoEspecial.Setup(repository => repository.GetAreasByUserIdAsync(4))
            .ReturnsAsync([]);

        var act = () => CrearServicio().AutorizarAsync(
            plantilla, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ValidacionAplicacion.ApplicationException>(act);
        Assert.Equal(ApplicationErrors.Forbidden, exception.Error);
    }

    private PlantillaCambioEstadoAuthorizationService CrearServicio() =>
        new(_currentUser.Object, _accesoEspecial.Object);

    private void ConfigurarUsuario(
        long id,
        RolUsuario rol,
        string codigoRol,
        long? area = null,
        long? areaPadre = null)
    {
        _currentUser.Setup(user => user.UserIdLocal).Returns(id);
        _currentUser.Setup(user => user.RolUsuario).Returns(rol);
        _currentUser.Setup(user => user.CodigoRolUsuario).Returns(codigoRol);
        _currentUser.Setup(user => user.UserAreaId).Returns(area);
        _currentUser.Setup(user => user.UserAreaPadreId).Returns(areaPadre);
    }

    private static Plantilla CrearPlantilla() =>
        Plantilla.Crear(1, "Plantilla", "Descripción", false, 10, "GTI", true);

    private static void AsignarArea(Plantilla plantilla, long area, long areaPadre)
    {
        typeof(Plantilla).GetProperty(nameof(Plantilla.IdAreaPlantilla))!
            .SetValue(plantilla, area);
        typeof(Plantilla).GetProperty(nameof(Plantilla.IdAreaPadrePlantilla))!
            .SetValue(plantilla, areaPadre);
    }
}
