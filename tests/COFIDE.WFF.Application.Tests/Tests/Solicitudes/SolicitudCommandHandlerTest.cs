using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ActivarDocumentoBloqueado;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AnularSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.ReusarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.In;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class SolicitudCommandHandlerTest
{
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICrearSolicitudUseCase> _crear = new();
    private readonly Mock<IEditarSolicitudUseCase> _editar = new();
    private readonly Mock<IReusarSolicitudUseCase> _reusar = new();
    private readonly Mock<IRevisarSolicitudUseCase> _revisar = new();
    private readonly Mock<IPlantillaRepository> _plantillas = new();
    private readonly Mock<ISolicitudRepository> _solicitudes = new();
    private readonly Mock<ICurrentUserService> _usuario = new();
    private readonly Mock<ISolicitudAnulacionService> _anulacion = new();
    private readonly Mock<IDetalleFirmaRepository> _detalles = new();

    public SolicitudCommandHandlerTest()
    {
        _plantillas.Setup(r => r.AddAsync(It.IsAny<Plantilla>())).Returns(Task.CompletedTask);
        _solicitudes.Setup(r => r.AddAsync(It.IsAny<Solicitud>())).Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<SolicitudRequestDto>(It.IsAny<object>()))
            .Returns(new SolicitudRequestDto(Asunto: "Contrato"));
    }

    [Fact]
    public async Task Registrar_Con_Plantilla_Debe_Persistir_Ambos_Y_Devolver_El_Codigo()
    {
        var solicitud = Borrador();
        var plantilla = NuevaPlantilla();
        _crear.Setup(u => u.Crear(It.IsAny<SolicitudRequestDto>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((solicitud, plantilla));

        var codigo = await new RegistrarSolicitudCommandHandler(
            _mapper.Object, _crear.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(ComandoRegistrar(), CancellationToken.None);

        codigo.Should().Be("GTI-SOL-00000001-2026");
        _plantillas.Verify(r => r.AddAsync(plantilla), Times.Once);
        _solicitudes.Verify(r => r.AddAsync(solicitud), Times.Once);
    }

    [Fact]
    public async Task Registrar_Sin_Plantilla_No_Debe_Persistir_Plantilla()
    {
        var solicitud = Borrador();
        _crear.Setup(u => u.Crear(It.IsAny<SolicitudRequestDto>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((solicitud, (Plantilla?)null));

        await new RegistrarSolicitudCommandHandler(
            _mapper.Object, _crear.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(ComandoRegistrar(), CancellationToken.None);

        _plantillas.Verify(r => r.AddAsync(It.IsAny<Plantilla>()), Times.Never);
        _solicitudes.Verify(r => r.AddAsync(solicitud), Times.Once);
    }

    [Fact]
    public async Task Enviar_Debe_Crear_En_Modo_No_Borrador()
    {
        var solicitud = Borrador();
        _crear.Setup(u => u.Crear(It.IsAny<SolicitudRequestDto>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((solicitud, (Plantilla?)null));

        var codigo = await new EnviarSolicitudCommandHandler(
            _mapper.Object, _crear.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(ComandoEnviar(), CancellationToken.None);

        codigo.Should().Be(solicitud.CodigoSolicitud);
        _crear.Verify(
            u => u.Crear(It.IsAny<SolicitudRequestDto>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Editar_Debe_Guardar_El_Flujo_Como_Borrador()
    {
        var solicitud = Borrador();
        var request = new SolicitudRequestDto(Asunto: "Editado");
        _editar.Setup(u => u.Editar(request, 8, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((solicitud, (Plantilla?)null));

        var codigo = await new EditarSolicitudCommandHandler(
            _mapper.Object, _editar.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(new EditarSolicitudCommand(8, request), CancellationToken.None);

        codigo.Should().Be(solicitud.CodigoSolicitud);
        _plantillas.Verify(r => r.GuardarFlujoPlantillaAsync(null), Times.Once);
        _solicitudes.Verify(r => r.GuardarFlujoSolicitudAsync(solicitud), Times.Once);
    }

    [Fact]
    public async Task EditarEnviar_Debe_Guardar_El_Flujo_Como_Envio()
    {
        var solicitud = Borrador();
        var plantilla = NuevaPlantilla();
        var request = new SolicitudRequestDto(Asunto: "Enviado");
        _editar.Setup(u => u.Editar(request, 8, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((solicitud, plantilla));

        var codigo = await new EditarEnviarSolicitudCommandHandler(
            _mapper.Object, _editar.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(new EditarEnviarSolicitudCommand(8, request), CancellationToken.None);

        codigo.Should().Be(solicitud.CodigoSolicitud);
        _plantillas.Verify(r => r.GuardarFlujoPlantillaAsync(plantilla), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Reusar_Debe_Persistir_La_Plantilla_Solo_Cuando_Existe(bool conPlantilla)
    {
        var solicitud = Borrador("GTI-SOL-00000009-2026");
        Plantilla? plantilla = conPlantilla ? NuevaPlantilla() : null;
        var request = new SolicitudRequestDto(Asunto: "Reuso");
        (Solicitud, Plantilla?) resultado = (solicitud, plantilla);
        _mapper.Setup(m => m.Map<SolicitudRequestDto>(request)).Returns(request);
        _reusar.Setup(u => u.Reusar(request, 4, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);

        var codigo = await new ReusarSolicitudCommandHandler(
            _mapper.Object, _reusar.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(new ReusarSolicitudCommand(4, request), CancellationToken.None);

        codigo.Should().Be("GTI-SOL-00000009-2026");
        _plantillas.Verify(
            r => r.AddAsync(It.IsAny<Plantilla>()),
            conPlantilla ? Times.Once() : Times.Never());
        _solicitudes.Verify(r => r.AddAsync(solicitud), Times.Once);
    }

    [Fact]
    public async Task ReusarEnviar_Debe_Reusar_En_Modo_No_Borrador()
    {
        var solicitud = Borrador();
        var request = new SolicitudRequestDto(Asunto: "Reuso enviado");
        _reusar.Setup(u => u.Reusar(request, 4, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((solicitud, (Plantilla?)null));

        await new ReusarEnviarSolicitudCommandHandler(
            _mapper.Object, _reusar.Object, _plantillas.Object, _solicitudes.Object)
            .Handle(new ReusarEnviarSolicitudCommand(4, request), CancellationToken.None);

        _reusar.Verify(
            u => u.Reusar(request, 4, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Aprobar_Debe_Delegar_En_El_Caso_De_Uso_Y_Devolver_Su_Resultado()
    {
        var cmd = new AprobarSolicitudCommand(15, "ok", null);
        _revisar.Setup(u => u.Aprobar(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GTI-SOL-00000015-2026");

        var codigo = await new AprobarSolicitudCommandHandler(_mapper.Object, _revisar.Object)
            .Handle(cmd, CancellationToken.None);

        codigo.Should().Be("GTI-SOL-00000015-2026");
    }

    [Fact]
    public async Task Rechazar_Debe_Delegar_En_El_Caso_De_Uso()
    {
        var cmd = new RechazarSolicitudCommand(15, "motivo", null, null);
        _revisar.Setup(u => u.Rechazar(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GTI-SOL-00000015-2026");

        var codigo = await new RechazarSolicitudCommandHandler(_mapper.Object, _revisar.Object)
            .Handle(cmd, CancellationToken.None);

        codigo.Should().Be("GTI-SOL-00000015-2026");
    }

    [Fact]
    public async Task Anular_Cuando_No_Existe_Debe_Lanzar_APP_SOL_001()
    {
        _solicitudes
            .Setup(r => r.ObtenerAgregadoSolicitudAsync(9, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Solicitud?)null);

        var act = () => new AnularSolicitudCommandHandler(
            _mapper.Object, _usuario.Object, _solicitudes.Object, _anulacion.Object)
            .Handle(new AnularSolicitudCommand(9), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.SolicitudInvalid.Code);
    }

    [Fact]
    public async Task Anular_Cuando_El_Usuario_No_Es_El_Solicitante_Debe_Rechazarse()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(99);
        _solicitudes
            .Setup(r => r.ObtenerAgregadoSolicitudAsync(1, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(Borrador(solicitante: 10));

        var act = () => new AnularSolicitudCommandHandler(
            _mapper.Object, _usuario.Object, _solicitudes.Object, _anulacion.Object)
            .Handle(new AnularSolicitudCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<AppEx.ApplicationException>();
        _anulacion.Verify(
            s => s.AnularSolicitudAsync(It.IsAny<Solicitud>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Anular_Del_Solicitante_Debe_Anular_Y_Devolver_El_Codigo()
    {
        var solicitud = Borrador(solicitante: 10);
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        _solicitudes
            .Setup(r => r.ObtenerAgregadoSolicitudAsync(1, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(solicitud);

        var codigo = await new AnularSolicitudCommandHandler(
            _mapper.Object, _usuario.Object, _solicitudes.Object, _anulacion.Object)
            .Handle(new AnularSolicitudCommand(1), CancellationToken.None);

        codigo.Should().Be(solicitud.CodigoSolicitud);
        _anulacion.Verify(
            s => s.AnularSolicitudAsync(solicitud, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivarDocumento_Sin_Detalle_Bloqueado_Debe_Lanzar_APP_SOL_012()
    {
        _detalles
            .Setup(r => r.ObtenerDetalleBloqueadoPorDocumentoAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DetalleFirma?)null);

        var act = () => new ActivarDocumentoBloqueadoCommandHandler(_mapper.Object, _detalles.Object)
            .Handle(new ActivarDocumentoBloqueadoCommand(3), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.DocumentoBloqueadoNotFound.Code);
    }

    [Fact]
    public async Task ActivarDocumento_Debe_Activar_El_Detalle_Y_Devolver_El_Nombre()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(
            7, "contrato", "pdf", "/r", "h",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        var detalle = DetalleFirma.Crear(1, 10, documento.Id, null, null, null, null, 100);
        detalle.SolicitudDocumento = documento;
        detalle.Bloquear();

        _detalles
            .Setup(r => r.ObtenerDetalleBloqueadoPorDocumentoAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detalle);

        var nombre = await new ActivarDocumentoBloqueadoCommandHandler(_mapper.Object, _detalles.Object)
            .Handle(new ActivarDocumentoBloqueadoCommand(7), CancellationToken.None);

        nombre.Should().Be("contrato");
        detalle.EstadoDocumento.Should().Be(Domain.ValueObjects.EstadoDocumento.Pendiente);
    }

    private static Solicitud Borrador(string codigo = "GTI-SOL-00000001-2026", long solicitante = 10) =>
        Solicitud.CrearBorrador(
            1, codigo, 1, 2, solicitante, 3, solicitante, 5, 2,
            "Contrato", null, false, false, null, 0);

    private static Plantilla NuevaPlantilla() =>
        Plantilla.Crear(1, "Flujo", "Descripcion", false, 10, "GTI", true);

    private static RegistrarSolicitudCommand ComandoRegistrar() =>
        new(1, 2, 3, "Contrato", null, false, false, null, null, null, null, null, null, null, null, null);

    private static EnviarSolicitudCommand ComandoEnviar() =>
        new(1, 2, 3, "Contrato", null, false, false, null, null, null, null, null, null, null, null, null);
}
