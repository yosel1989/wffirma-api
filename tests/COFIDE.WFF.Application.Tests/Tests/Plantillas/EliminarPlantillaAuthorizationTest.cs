using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EliminarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Exceptions;
using Moq;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Tests.Plantillas;

public class EliminarPlantillaAuthorizationTest
{
    [Fact]
    public async Task Handle_SinUsuarioAutenticado_Deniega_RF02()
    {
        var repository = CrearRepositorio(CrearPlantilla());
        var currentUser = new Mock<ICurrentUserService>();
        var handler = CrearHandler(repository, currentUser, new Mock<IAccesoEspecialRepository>());

        var exception = await Assert.ThrowsAsync<ValidacionAplicacion.ApplicationException>(
            () => handler.Handle(
                new EliminarPlantillaCommand(1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrors.UserNotAuthenticated, exception.Error);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Plantilla>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConAdministrador_Permite_RF02()
    {
        await AssertPermiteAsync(
            ConfigurarUsuario(1, RolUsuario.Administrador,
                RolUsuarioCodigo.Administrador));
    }

    [Fact]
    public async Task Handle_ConCoordinadorDeArea_Permite_RF02()
    {
        var currentUser = ConfigurarUsuario(2, RolUsuario.Coordinador,
            RolUsuarioCodigo.Coordinador, 100, 200);
        var plantilla = CrearPlantilla();
        AsignarArea(plantilla, 100, 200);

        await AssertPermiteAsync(currentUser, plantilla);
    }

    [Fact]
    public async Task Handle_ConCoordinadorDeAccesoEspecial_Permite_RF02()
    {
        var currentUser = ConfigurarUsuario(3, RolUsuario.Coordinador,
            RolUsuarioCodigo.Coordinador, 300, 400);
        var acceso = new Mock<IAccesoEspecialRepository>();
        acceso.Setup(repo => repo.GetAreasByUserIdAsync(3)).ReturnsAsync([100]);
        var plantilla = CrearPlantilla();
        AsignarArea(plantilla, 100, 200);

        await AssertPermiteAsync(currentUser, plantilla, acceso);
    }

    [Fact]
    public async Task Handle_ConSolicitantePropietario_Permite_RF02()
    {
        await AssertPermiteAsync(
            ConfigurarUsuario(10, RolUsuario.SolicitanteAprobador,
                RolUsuarioCodigo.SolicitanteAprobador));
    }

    [Fact]
    public async Task Handle_ConCoordinadorSinAlcance_Deniega_RF02()
    {
        var currentUser = ConfigurarUsuario(4, RolUsuario.Coordinador,
            RolUsuarioCodigo.Coordinador, 300, 400);
        var acceso = new Mock<IAccesoEspecialRepository>();
        acceso.Setup(repo => repo.GetAreasByUserIdAsync(4)).ReturnsAsync([]);
        var plantilla = CrearPlantilla();
        AsignarArea(plantilla, 100, 200);
        var repository = CrearRepositorio(plantilla);
        var handler = CrearHandler(repository, currentUser, acceso);

        var exception = await Assert.ThrowsAsync<ValidacionAplicacion.ApplicationException>(
            () => handler.Handle(
                new EliminarPlantillaCommand(1),
                CancellationToken.None));

        Assert.Equal(ApplicationErrors.Forbidden, exception.Error);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Plantilla>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConSolicitanteNoPropietario_Deniega_RF02()
    {
        var currentUser = ConfigurarUsuario(11, RolUsuario.SolicitanteAprobador,
            RolUsuarioCodigo.SolicitanteAprobador);
        var repository = CrearRepositorio(CrearPlantilla());
        var handler = CrearHandler(
            repository, currentUser, new Mock<IAccesoEspecialRepository>());

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(
                new EliminarPlantillaCommand(1),
                CancellationToken.None));

        Assert.Equal("DOM_PLT_027", exception.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<Plantilla>()), Times.Never);
    }

    private async Task AssertPermiteAsync(
        Mock<ICurrentUserService> currentUser,
        Plantilla? plantilla = null,
        Mock<IAccesoEspecialRepository>? acceso = null)
    {
        plantilla ??= CrearPlantilla();
        var repository = CrearRepositorio(plantilla);
        var handler = CrearHandler(
            repository,
            currentUser,
            acceso ?? new Mock<IAccesoEspecialRepository>());

        var resultado = await handler.Handle(
            new EliminarPlantillaCommand(1),
            CancellationToken.None);

        Assert.True(resultado);
        repository.Verify(repo => repo.UpdateAsync(plantilla), Times.Once);
    }

    private static EliminarPlantillaCommandHandler CrearHandler(
        Mock<IPlantillaRepository> repository,
        Mock<ICurrentUserService> currentUser,
        Mock<IAccesoEspecialRepository> acceso) =>
        new(
            Mock.Of<IMapper>(),
            repository.Object,
            new PlantillaCambioEstadoAuthorizationService(
                currentUser.Object,
                acceso.Object));

    private static Mock<IPlantillaRepository> CrearRepositorio(Plantilla plantilla)
    {
        var repository = new Mock<IPlantillaRepository>();
        repository.Setup(repo => repo.ObtenerParaCambioEstadoAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        return repository;
    }

    private static Mock<ICurrentUserService> ConfigurarUsuario(
        long id,
        RolUsuario rol,
        string codigoRol,
        long? area = null,
        long? areaPadre = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(user => user.UserIdLocal).Returns(id);
        currentUser.Setup(user => user.RolUsuario).Returns(rol);
        currentUser.Setup(user => user.CodigoRolUsuario).Returns(codigoRol);
        currentUser.Setup(user => user.UserAreaId).Returns(area);
        currentUser.Setup(user => user.UserAreaPadreId).Returns(areaPadre);
        return currentUser;
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
