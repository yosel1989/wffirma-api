using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class SolicitudReusoServiceTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Reusar_Debe_Copiar_Referencias_Y_Dejar_El_Estado_Segun_El_Envio(bool esBorrador)
    {
        var origen = Borrador("GTI-SOL-00000004-2026");
        var nueva = Borrador("GTI-SOL-00000009-2026");
        var solicitudes = new Mock<ISolicitudRepository>();
        var creacion = new Mock<ISolicitudCreacionService>();
        var documentos = new Mock<ISolicitudDocumentoService>();
        var sustentos = new Mock<ISolicitudSustentoService>();
        var plantillas = new Mock<ISolicitudPlantillaService>();
        var etiquetas = new Mock<ISolicitudDocumentoTagService>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var request = new SolicitudRequestDto(
            IdTipoSolicitud: 1,
            Asunto: "Reuso",
            CodigosReferencia: ["REF-1"],
            Correos: ["ana@cofide.com.pe"]);

        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(4, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(origen);
        creacion.Setup(c => c.CrearSolicitudBase(request, "GTI", 10, 5, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nueva);
        documentos.Setup(d => d.CrearDocumentoInput(null, nueva.CodigoSolicitud, false, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentoProcesadoInput>());
        sustentos.Setup(s => s.CrearDocumentoSustentoInput(null, nueva.CodigoSolicitud, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SustentoProcesadoInput>());
        plantillas.Setup(p => p.CrearSolicitudPlantilla(0, null, null, 10, "GERENCIA GENERAL", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plantilla?)null);
        plantillas.Setup(p => p.CrearFlujoDesdeSolicitud(null, nueva, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        etiquetas.Setup(e => e.CrearDocumentoTagDesdeSolicitud(nueva, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdCodigosReferenciaAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 11 });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdCorreosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 12 });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDocumentoAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdSustentoAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long>());

        var (solicitud, plantilla) = await new SolicitudReusoService(
            Usuario(),
            solicitudes.Object,
            creacion.Object,
            documentos.Object,
            sustentos.Object,
            plantillas.Object,
            etiquetas.Object,
            correlativos.Object)
            .Reusar(request, 4, esBorrador, CancellationToken.None);

        plantilla.Should().BeNull();
        solicitud.CodigoSolicitud.Should().Be("GTI-SOL-00000009-2026");
        solicitud.CodigosReferencia.Should().ContainSingle(c => c.CodigoReferencia.Valor == "REF-1" && c.Id == 11);
        solicitud.Correos.Should().ContainSingle(c => c.Correo.Value == "ana@cofide.com.pe" && c.Id == 12);
        if (esBorrador)
        {
            solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.Borrador);
            solicitud.DomainEvents.Should().ContainSingle(e => e is SolicitudCreadaBorradorDomainEvent);
        }
        else
        {
            solicitud.EstadoSolicitud.Should().Be(EstadoSolicitud.EnCurso);
            solicitud.Asunto.Should().Be("Reuso");
            solicitud.DomainEvents.Should().Contain(e => e is SolicitudCreadaDomainEvent);
        }
    }

    private static ICurrentUserService Usuario()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10L);
        usuario.Setup(u => u.UserAreaId).Returns(5L);
        usuario.Setup(u => u.UserAreaPadreId).Returns(2L);
        usuario.Setup(u => u.UserAreaDescripcionCorta).Returns("GTI");
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GERENCIA GENERAL");
        usuario.Setup(u => u.UserCorreo).Returns("ana@cofide.com.pe");
        return usuario.Object;
    }

    private static Solicitud Borrador(string codigo) =>
        Solicitud.CrearBorrador(codigo.Contains("00000009") ? 9 : 4, codigo, 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
