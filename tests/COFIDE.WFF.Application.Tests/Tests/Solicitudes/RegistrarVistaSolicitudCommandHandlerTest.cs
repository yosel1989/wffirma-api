using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarVistaSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Tiempo;
using Moq;
using AppException = COFIDE.WFF.Application.Exceptions.ApplicationException;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class RegistrarVistaSolicitudCommandHandlerTest
{
    private readonly Mock<ICurrentUserService> _usuario = new();
    private readonly Mock<ISolicitudRepository> _solicitudes = new();

    private RegistrarVistaSolicitudCommandHandler Handler() => new(
        new Mock<IMapper>().Object, _usuario.Object, _solicitudes.Object);

    private List<SolicitudFirmanteFlujo> PrepararFirmantes()
    {
        var firmantes = new List<SolicitudFirmanteFlujo>
        {
            SolicitudFirmanteFlujo.Crear(1, 2, 3, 4, 2, 100),
            SolicitudFirmanteFlujo.Crear(2, 3, 3, 5, 3, 100)
        };
        firmantes[0].Aprobar();
        _usuario.Setup(x => x.UserIdLocal).Returns(100);
        _solicitudes.Setup(x => x.ObtenerFirmantesParaVisualizacionAsync(8, 100, It.IsAny<CancellationToken>())).ReturnsAsync(firmantes);
        return firmantes;
    }

    [Fact]
    public async Task Handle_UsaUsuarioActualSinExigirTurnoNiEstadoPendienteYConservaPrimeraFecha()
    {
        var firmantes = PrepararFirmantes();
        var fechaRevision = firmantes[0].FechaRevision;
        var fecha = new DateTime(2026, 11, 17, 12, 0, 0);
        using var cancelacion = new CancellationTokenSource();
        using (HoraSistema.Fijar(fecha))
            Assert.Equal(fecha, await Handler().Handle(new(8), cancelacion.Token));
        using (HoraSistema.Fijar(fecha.AddMinutes(5)))
            Assert.Equal(fecha, await Handler().Handle(new(8), cancelacion.Token));

        Assert.All(firmantes, f => Assert.Equal(fecha, f.FechaVistaSolicitud));
        Assert.True(firmantes[0].RevAprobado);
        Assert.Equal(fechaRevision, firmantes[0].FechaRevision);
        _solicitudes.Verify(x => x.ObtenerFirmantesParaVisualizacionAsync(8, 100, cancelacion.Token), Times.Exactly(2));
        _solicitudes.Verify(x => x.GuardarFlujoSolicitudAsync(It.IsAny<Solicitud>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SinUsuarioNoConsultaNiActualiza()
    {
        var error = await Assert.ThrowsAsync<AppException>(() => Handler().Handle(new(8), default));

        Assert.Equal("AUTH_REQUIRED", error.Error.Code);
        _solicitudes.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_SinFirmanteAsignadoRechazaLaOperacion()
    {
        var firmantes = PrepararFirmantes();
        _solicitudes.Setup(x => x.ObtenerFirmantesParaVisualizacionAsync(8, 100, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var error = await Assert.ThrowsAsync<AppException>(() => Handler().Handle(new(8), default));

        Assert.Equal("APP_SOL_023", error.Error.Code);
        Assert.All(firmantes, f => Assert.Null(f.FechaVistaSolicitud));
    }

    [Fact]
    public async Task Handle_CancelacionNoMarcaLaSolicitud()
    {
        var firmantes = PrepararFirmantes();
        using var cancelacion = new CancellationTokenSource();
        cancelacion.Cancel();
        _solicitudes.Setup(x => x.ObtenerFirmantesParaVisualizacionAsync(8, 100, cancelacion.Token))
            .ThrowsAsync(new OperationCanceledException(cancelacion.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Handler().Handle(new(8), cancelacion.Token));

        Assert.All(firmantes, f => Assert.Null(f.FechaVistaSolicitud));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(8, true)]
    public void Validator_ExigeIdentificadorPositivo(long id, bool valido)
    {
        var resultado = new RegistrarVistaSolicitudValidator().Validate(new RegistrarVistaSolicitudCommand(id));
        Assert.Equal(valido, resultado.IsValid);
        if (!valido)
            Assert.Equal("APP_SOL_001", Assert.Single(resultado.Errors).ErrorCode);
    }
}
