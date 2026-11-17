using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ContarSolicitudesNoVistas;
using Moq;
using AppException = COFIDE.WFF.Application.Exceptions.ApplicationException;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class ContarSolicitudesNoVistasQueryHandlerTest
{
    private readonly Mock<ICurrentUserService> _usuario = new();
    private readonly Mock<ISolicitudQueryRepository> _consultas = new();

    private ContarSolicitudesNoVistasQueryHandler Handler() => new(
        new Mock<IMapper>().Object, _usuario.Object, _consultas.Object);

    [Fact]
    public async Task Handle_DevuelveElConteoDelUsuarioAutenticado()
    {
        using var cancelacion = new CancellationTokenSource();
        _usuario.Setup(x => x.UserIdLocal).Returns(100);
        _consultas.Setup(x => x.ContarSolicitudesPorAprobarNoVistas(100, cancelacion.Token)).ReturnsAsync(3);

        Assert.Equal(3, await Handler().Handle(new(), cancelacion.Token));
    }

    [Fact]
    public async Task Handle_SinUsuarioNoConsulta()
    {
        var error = await Assert.ThrowsAsync<AppException>(() => Handler().Handle(new(), default));

        Assert.Equal("AUTH_REQUIRED", error.Error.Code);
        _consultas.VerifyNoOtherCalls();
    }
}
