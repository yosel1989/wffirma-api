using COFIDE.WFF.Application.Common.Files;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Tests.Support;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

#region Datos del caso (mapean el JSON de Resources)
public sealed record UsuarioActualData(
long UserIdLocal,
long UserAreaId,
long UserAreaPadreId,
string UserAreaDescripcionCorta,
string UserGerenciaNombre,
string UserCorreo);
public sealed record SolicitudRegistroServiceCase(string Caso,
bool EsBorrador,
string CodigoSolicitud,
List<string> CodigosReferencia,
List<string> Correos,
long? IdPlantilla,
string? Configuracion,
bool ConPlantilla,
long IdEstadoEsperado,
string EventoEsperado)
{
    public override string ToString() => Caso;
}
public sealed record UsuarioSolicitudData(UsuarioActualData UsuarioActual);
#endregion
public class SolicitudRegistroServiceTest
{
    private static readonly UsuarioActualData Usuario =
        JsonResource.Single<UsuarioSolicitudData>("Solicitudes/usuario-solicitud-usecase.json").UsuarioActual;

    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<SolicitudRegistroServiceCase>("Solicitudes/crear-solicitud-usecase.json");

    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ISolicitudCreacionService> _creacion = new();
    private readonly Mock<ISolicitudDocumentoService> _documentos = new();
    private readonly Mock<ISolicitudSustentoService> _sustentos = new();
    private readonly Mock<ISolicitudPlantillaService> _plantillas = new();
    private readonly Mock<ISolicitudDocumentoTagService> _tags = new();
    private readonly Mock<IGeneradorCorrelativoIdRepository> _correlativos = new();
    public SolicitudRegistroServiceTest()
    {
        var usuario = Usuario;
        _currentUser.Setup(x => x.UserIdLocal).Returns(usuario.UserIdLocal);
        _currentUser.Setup(x => x.UserAreaId).Returns(usuario.UserAreaId);
        _currentUser.Setup(x => x.UserAreaPadreId).Returns(usuario.UserAreaPadreId);
        _currentUser.Setup(x => x.UserAreaDescripcionCorta).Returns(usuario.UserAreaDescripcionCorta);
        _currentUser.Setup(x => x.UserGerenciaNombre).Returns(usuario.UserGerenciaNombre);
        _currentUser.Setup(x => x.UserCorreo).Returns(usuario.UserCorreo);

        _correlativos
         .Setup(x => x.ObtenerBloqueSiguienteIdCodigosReferenciaAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync((long cantidad, CancellationToken _) => Rango(100, cantidad));

        _correlativos
        .Setup(x => x.ObtenerBloqueSiguienteIdCorreosAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((long cantidad, CancellationToken _) => Rango(200, cantidad));

        _correlativos
        .Setup(x => x.ObtenerBloqueSiguienteIdDocumentoAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((long cantidad, CancellationToken _) => Rango(300, cantidad));

        _correlativos
        .Setup(x => x.ObtenerBloqueSiguienteIdSustentoAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((long cantidad, CancellationToken _) => Rango(400, cantidad));

        _documentos
         .Setup(x => x.CrearDocumentoInput(
            It.IsAny<List<ArchivoEntrada>?>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<DocumentoProcesadoInput>());

        _sustentos
        .Setup(x => x.CrearDocumentoSustentoInput(
            It.IsAny<List<ArchivoEntrada>?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<SustentoProcesadoInput>());

    }

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task CrearDebe_Orquestar_Servicios_Y_Dejar_El_Estado_Esperado(SolicitudRegistroServiceCase caso)
    {
        var usuario = Usuario;
        var solicitudBase = CrearSolicitudBorrador(caso.CodigoSolicitud);
        Plantilla? plantilla = caso.ConPlantilla ? CrearPlantilla() : null;

        _creacion
            .Setup(x => x.CrearSolicitudBase(
                It.IsAny<SolicitudRequestDto>(),
                usuario.UserAreaDescripcionCorta,
                usuario.UserIdLocal, usuario.UserAreaId,
                usuario.UserAreaPadreId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitudBase);

        _plantillas
            .Setup(x => x.CrearSolicitudPlantilla(
                0,
                caso.Configuracion,
                caso.IdPlantilla,
                usuario.UserIdLocal,
                usuario.UserGerenciaNombre,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);


        var request = new SolicitudRequestDto(IdTipoSolicitud: 1,
        IdSubTipoSolicitud: 2,
        IdtipoOperacion: 3,
        Asunto: "Contrato de fideicomiso",
        CodigosReferencia: caso.CodigosReferencia,
        Correos: caso.Correos,
        IdPlantilla: caso.IdPlantilla,
        Configuracion: caso.Configuracion);

        var sut = CrearSut();
        // Act
        var (solicitud, plantillaCreada) = await sut.Crear(request, caso.EsBorrador, CancellationToken.None); // Assert

        solicitud.IdEstadoSolicitud.Should().Be(caso.IdEstadoEsperado);
        solicitud.CodigosReferencia.Should().HaveCount(caso.CodigosReferencia.Count);
        solicitud.Correos.Should().HaveCount(caso.Correos.Count);
        solicitud.DomainEvents.Select(e => e.GetType().Name).Should().Contain(caso.EventoEsperado);
        plantillaCreada.Should().BeSameAs(plantilla);

        _correlativos.Verify(
        x => x.ObtenerBloqueSiguienteIdCodigosReferenciaAsync(
        (long)caso.CodigosReferencia.Count, It.IsAny<CancellationToken>()), Times.Once);

        _correlativos.Verify(
        x => x.ObtenerBloqueSiguienteIdCorreosAsync(
        (long)caso.Correos.Count, It.IsAny<CancellationToken>()), Times.Once);

        _plantillas.Verify(
        x => x.CrearFlujoDesdeSolicitud(plantilla, solicitud, It.IsAny<CancellationToken>()), Times.Once);
        _tags.Verify(
        x => x.CrearDocumentoTagDesdeSolicitud(solicitud, request.Tags, It.IsAny<CancellationToken>()), Times.Once);

    }


    [Fact]
    public async Task CrearDebe_Usar_El_Codigo_De_Area_Y_Usuario_Del_Contexto_Actual()
    {
        var usuario = Usuario;
        _creacion
            .Setup(x => x.CrearSolicitudBase(
                It.IsAny<SolicitudRequestDto>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearSolicitudBorrador("GTI-SOL-00000003-2026"));

        _plantillas
            .Setup(x => x.CrearSolicitudPlantilla(
                It.IsAny<long?>(),
                It.IsAny<string?>(),
                It.IsAny<long?>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plantilla?)null);

        await CrearSut().Crear(new SolicitudRequestDto(Asunto: "Contrato"), true, CancellationToken.None);

        _creacion.Verify(
            x => x.CrearSolicitudBase(
                It.IsAny<SolicitudRequestDto>(),
                usuario.UserAreaDescripcionCorta,
                usuario.UserIdLocal, usuario.UserAreaId,
                usuario.UserAreaPadreId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region Helpers
    private SolicitudRegistroService CrearSut() => new(
    _currentUser.Object,
    _creacion.Object,
    _documentos.Object,
    _sustentos.Object,
    _plantillas.Object,
    _tags.Object,
    _correlativos.Object);
    private static List<long> Rango(long inicio, long cantidad) => Enumerable.Range(0, (int)cantidad).Select(i => inicio + i).ToList();
    private static Solicitud CrearSolicitudBorrador(string codigo) =>
        Solicitud.CrearBorrador(
            id: 1,
            codigoSolicitud: codigo,
            idTipoSolicitud: 1,
            idSubTipoSolicitud: 2,
            idUsuarioSolicitante: 10,
            idTipoOperacion: 3,
            idUsuarioAccion: 10,
            idAreaSolicitud: 5,
            idAreaPadreSolicitud: 2,
            asunto: "Asunto de prueba",
            mensaje: "Mensaje de prueba",
            esUrgente: false,
            inCodigo: false,
            fechaLimite: null,
            pesoTotalDocu: 0);

    private static Plantilla CrearPlantilla() =>
        Plantilla.Crear(1, "Flujo de prueba", "Descripcion", false, 10, "GERENCIA GENERAL", false);
    #endregion


}
