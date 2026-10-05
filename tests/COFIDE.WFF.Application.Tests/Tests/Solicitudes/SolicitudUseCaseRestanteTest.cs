using COFIDE.WFF.Application.Common.Files;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Features.Solicitudes.UseCases;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class SolicitudUseCaseRestanteTest
{
    [Fact]
    public async Task Editar_Sin_Solicitud_Debe_Lanzar_APP_SOL_001()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Solicitud?)null);

        var act = () => Editar(solicitudes).Editar(new SolicitudRequestDto(Asunto: "Editado"), 8, true, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_001");
    }

    [Fact]
    public async Task Editar_De_Otro_Solicitante_Debe_Lanzar_DomainException()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(Borrador());

        var act = () => Editar(solicitudes, usuarioId: 99).Editar(new SolicitudRequestDto(Asunto: "Editado"), 8, true, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Editar_Borrador_Del_Dueno_Debe_Actualizar_El_Asunto_Y_La_Plantilla()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<ISolicitudPlantillaService>();
        var documentos = new Mock<ISolicitudDocumentoService>();
        var sustentos = new Mock<ISolicitudSustentoService>();
        var tags = new Mock<ISolicitudDocumentoTagService>();
        var plantilla = Plantilla.Crear(1, "Flujo", "Descripcion", false, 10, "GTI", true);
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(Borrador());
        documentos.Setup(d => d.CrearDocumentoInput(It.IsAny<List<ArchivoEntrada>?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentoProcesadoInput>());
        sustentos.Setup(s => s.CrearDocumentoSustentoInput(It.IsAny<List<ArchivoEntrada>?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SustentoProcesadoInput>());
        plantillas.Setup(p => p.CrearSolicitudPlantilla(0, null, null, 10, "GTI", It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        plantillas.Setup(p => p.ActualizarFlujoDesdeSolicitud(plantilla, It.IsAny<Solicitud>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        tags.Setup(t => t.AntualizarDocumentoTagDesdeSolicitud(It.IsAny<Solicitud>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        documentos.Setup(d => d.SincronizarTagDocumentoAsync(It.IsAny<Solicitud>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var (solicitud, plantillaCreada) = await Editar(solicitudes, plantillas, documentos, sustentos, tags)
            .Editar(new SolicitudRequestDto(IdTipoSolicitud: 1, IdSubTipoSolicitud: 2, Asunto: "Editado"), 8, true, CancellationToken.None);

        solicitud.Asunto.Should().Be("Editado");
        plantillaCreada.Should().BeSameAs(plantilla);
        solicitud.DomainEvents.Should().Contain(e => e.GetType().Name == "SolicitudCreadaBorradorDomainEvent");
    }

    [Fact]
    public async Task Editar_Y_Enviar_Debe_Pasar_La_Solicitud_A_En_Curso()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<ISolicitudPlantillaService>();
        var documentos = new Mock<ISolicitudDocumentoService>();
        var sustentos = new Mock<ISolicitudSustentoService>();
        var tags = new Mock<ISolicitudDocumentoTagService>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(Borrador());
        documentos.Setup(d => d.CrearDocumentoInput(It.IsAny<List<ArchivoEntrada>?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentoProcesadoInput>());
        sustentos.Setup(s => s.CrearDocumentoSustentoInput(It.IsAny<List<ArchivoEntrada>?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SustentoProcesadoInput>());
        plantillas.Setup(p => p.CrearSolicitudPlantilla(0, null, null, 10, "GTI", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plantilla?)null);
        plantillas.Setup(p => p.ActualizarFlujoDesdeSolicitud(null, It.IsAny<Solicitud>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        tags.Setup(t => t.AntualizarDocumentoTagDesdeSolicitud(It.IsAny<Solicitud>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        documentos.Setup(d => d.SincronizarTagDocumentoAsync(It.IsAny<Solicitud>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var (solicitud, plantilla) = await Editar(solicitudes, plantillas, documentos, sustentos, tags)
            .Editar(new SolicitudRequestDto(IdTipoSolicitud: 1, IdSubTipoSolicitud: 2, Asunto: "Enviado"), 8, false, CancellationToken.None);

        plantilla.Should().BeNull();
        solicitud.Asunto.Should().Be("Enviado");
        solicitud.EstadoSolicitud.Should().Be(COFIDE.WFF.Domain.ValueObjects.EstadoSolicitud.EnCurso);
        solicitud.DomainEvents.Should().Contain(e => e.GetType().Name == "SolicitudCreadaDomainEvent");
    }

    [Fact]
    public async Task Reusar_Sin_Solicitud_Origen_Debe_Lanzar_APP_SOL_013()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((Solicitud?)null);

        var act = () => Reusar(solicitudes).Reusar(new SolicitudRequestDto(Asunto: "Reuso"), 4, true, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_013");
    }

    private static EditarSolicitudUseCase Editar(
        Mock<ISolicitudRepository> solicitudes,
        Mock<ISolicitudPlantillaService>? plantillas = null,
        Mock<ISolicitudDocumentoService>? documentos = null,
        Mock<ISolicitudSustentoService>? sustentos = null,
        Mock<ISolicitudDocumentoTagService>? tags = null,
        long usuarioId = 10) =>
        new(
            Usuario(usuarioId).Object,
            solicitudes.Object,
            (plantillas ?? new Mock<ISolicitudPlantillaService>()).Object,
            (documentos ?? new Mock<ISolicitudDocumentoService>()).Object,
            (sustentos ?? new Mock<ISolicitudSustentoService>()).Object,
            (tags ?? new Mock<ISolicitudDocumentoTagService>()).Object,
            Correlativos());

    private static ReusarSolicitudUseCase Reusar(Mock<ISolicitudRepository> solicitudes) =>
        new(
            Usuario(10).Object,
            solicitudes.Object,
            new Mock<ISolicitudCreacionService>().Object,
            new Mock<ISolicitudDocumentoService>().Object,
            new Mock<ISolicitudSustentoService>().Object,
            new Mock<ISolicitudPlantillaService>().Object,
            new Mock<ISolicitudDocumentoTagService>().Object,
            Correlativos());

    private static IGeneradorCorrelativoIdRepository Correlativos()
    {
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdCodigosReferenciaAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdCorreosAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDocumentoAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdSustentoAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<long>());
        return correlativos.Object;
    }

    private static Mock<ICurrentUserService> Usuario(long id)
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(id);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
        usuario.Setup(u => u.UserCorreo).Returns("ana@cofide.com.pe");
        usuario.Setup(u => u.UserAreaId).Returns(5);
        usuario.Setup(u => u.UserAreaPadreId).Returns(2);
        usuario.Setup(u => u.UserAreaDescripcionCorta).Returns("GTI");
        usuario.Setup(u => u.NombreUsuario).Returns("Ana");
        usuario.Setup(u => u.ApellidoPaterno).Returns("Perez");
        usuario.Setup(u => u.ApellidoMaterno).Returns("Lopez");
        return usuario;
    }

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
