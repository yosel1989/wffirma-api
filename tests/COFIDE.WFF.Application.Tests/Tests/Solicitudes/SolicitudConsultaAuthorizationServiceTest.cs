using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public sealed class SolicitudConsultaAuthorizationServiceTest
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ISolicitudQueryRepository> _solicitudQueryRepository = new();
    private readonly Mock<IAccesoEspecialRepository> _accesoEspecialRepository = new();

    [Fact]
    public async Task AutorizarPorIdAsync_ConAccesoEspecial_DebePermitirConsulta()
    {
        const long idUsuario = 20;
        const long idSolicitud = 8;
        using var cancellation = new CancellationTokenSource();
        ConfigurarUsuario(idUsuario, "SOLAPR");
        _solicitudQueryRepository
            .Setup(r => r.ObtenerParaAutorizacionAsync(idSolicitud, cancellation.Token))
            .ReturnsAsync(CrearSolicitud());
        _accesoEspecialRepository
            .Setup(r => r.GetAreasByUserIdAsync(idUsuario))
            .ReturnsAsync([5]);

        var service = CrearServicio();

        await service.AutorizarPorIdAsync(idSolicitud, cancellation.Token);

        _solicitudQueryRepository.Verify(
            r => r.ObtenerParaAutorizacionAsync(idSolicitud, cancellation.Token), Times.Once);
        _accesoEspecialRepository.Verify(r => r.GetAreasByUserIdAsync(idUsuario), Times.Once);
    }

    [Fact]
    public async Task AutorizarPorIdAsync_SinAccesoEspecial_DebeDenegarConsulta()
    {
        ConfigurarUsuario(20, "SOLAPR");
        _solicitudQueryRepository
            .Setup(r => r.ObtenerParaAutorizacionAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearSolicitud());
        _accesoEspecialRepository
            .Setup(r => r.GetAreasByUserIdAsync(20))
            .ReturnsAsync([]);

        var act = () => CrearServicio().AutorizarPorIdAsync(8, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.Error.Code.Should().Be("DOM_SOL_034");
    }

    [Fact]
    public async Task AutorizarReusoPorIdAsync_DesdeAccesoEspecial_DebeDenegar()
    {
        ConfigurarUsuario(20, "SOLAPR");
        _currentUser.Setup(u => u.RolUsuario)
            .Returns(COFIDE.WFF.Domain.Common.Enums.RolUsuario.SolicitanteAprobador);
        _solicitudQueryRepository
            .Setup(r => r.ObtenerParaAutorizacionAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearSolicitud());
        _accesoEspecialRepository
            .Setup(r => r.GetAreasByUserIdAsync(20))
            .ReturnsAsync([5]);

        var act = () => CrearServicio().AutorizarReusoPorIdAsync(8, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.Error.Code.Should().Be("DOM_SOL_050");
    }

    [Fact]
    public async Task AutorizarPorIdAsync_SinSolicitud_DebeLanzarSolicitudInvalid()
    {
        ConfigurarUsuario(20, "SOLAPR");
        _solicitudQueryRepository
            .Setup(r => r.ObtenerParaAutorizacionAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Solicitud?)null);

        var act = () => CrearServicio().AutorizarPorIdAsync(8, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        exception.Which.Error.Code.Should().Be("APP_SOL_001");
        _accesoEspecialRepository.Verify(r => r.GetAreasByUserIdAsync(It.IsAny<long>()), Times.Never);
    }

    private SolicitudConsultaAuthorizationService CrearServicio() =>
        new(_currentUser.Object, _solicitudQueryRepository.Object, _accesoEspecialRepository.Object);

    private void ConfigurarUsuario(long idUsuario, string codigoRol)
    {
        _currentUser.Setup(u => u.UserIdLocal).Returns(idUsuario);
        _currentUser.Setup(u => u.UserAreaId).Returns(99);
        _currentUser.Setup(u => u.CodigoRolUsuario).Returns(codigoRol);
    }

    private static Solicitud CrearSolicitud() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
