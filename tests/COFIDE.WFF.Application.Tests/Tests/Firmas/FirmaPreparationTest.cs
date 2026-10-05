using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Services;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Firmas;

public class FirmaPreparationTest
{
    [Fact]
    public async Task Firmante_Debe_Usar_La_Esquina_Y_El_Texto_De_Autor()
    {
        var documento = Documento(esFirmante: true);

        var solicitud = await Preparar(documento, cantidad: 1);

        solicitud.TipoFirma.Should().Be(1);
        solicitud.EsFirmante.Should().BeTrue();
        solicitud.EsVisador.Should().BeFalse();
        solicitud.Tags.Should().Be("[F]");
        solicitud.VisSigWidth.Should().Be(175);
        solicitud.VisSigHeight.Should().Be(40);
        solicitud.VisSigTextSize.Should().Be(8);
        solicitud.TiempoEspera.Should().Be(5000);
        solicitud.VisSigText.Should().Contain("Soy el autor del documento");
        solicitud.CodigoArchivo.Should().Be("REF-1");
    }

    [Fact]
    public async Task Visador_Debe_Usar_Coordenadas_Y_El_Texto_De_Visto_Bueno()
    {
        var documento = Documento(esFirmante: false);

        var solicitud = await Preparar(documento, cantidad: 1);

        solicitud.TipoFirma.Should().Be(4);
        solicitud.EsVisador.Should().BeTrue();
        solicitud.VisSigPage.Should().Be(1);
        solicitud.VisSigX.Should().Be(30);
        solicitud.VisSigY.Should().Be(40);
        solicitud.VisSigWidth.Should().Be(80);
        solicitud.VisSigText.Should().Contain("Doy Vo Bo");
    }

    [Fact]
    public async Task Firmante_Masivo_Debe_Firmar_Con_Tag()
    {
        var documento = Documento(esFirmante: true);

        var solicitud = await Preparar(documento, cantidad: 2);

        solicitud.TipoFirma.Should().Be(3);
        solicitud.TiempoEspera.Should().Be(7000);
        solicitud.Tags.Should().Be("[F]");
    }

    [Fact]
    public async Task Firmante_Con_Tag_En_El_Documento_Debe_Firmar_Con_Tag()
    {
        var documento = Documento(esFirmante: true);
        documento.AgregarDocumentoTag(8, 1, 10, 20, 100, 40);

        var solicitud = await Preparar(documento, cantidad: 1);

        solicitud.TipoFirma.Should().Be(3);
        solicitud.Tags.Should().Be("[F]");
        solicitud.VisSigPage.Should().BeNull();
    }

    [Fact]
    public async Task Visador_Masivo_Debe_Usar_La_Primera_Pagina()
    {
        var documento = Documento(esFirmante: false);

        var solicitud = await Preparar(documento, cantidad: 2, new InformacionPaginaPdfDto(800, 600, true));

        solicitud.TipoFirma.Should().Be(4);
        solicitud.VisSigPage.Should().Be(1);
        solicitud.Tags.Should().BeNull();
        solicitud.EsVisador.Should().BeTrue();
    }

    private static async Task<FirmarDocumentoRequestDto> Preparar(
        SolicitudDocumento documento,
        int cantidad,
        InformacionPaginaPdfDto? pagina = null)
    {
        var plantillas = new Mock<IPlantillaRepository>();
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(It.IsAny<long>(), It.IsAny<CancellationToken>(), true))
            .ReturnsAsync(documento.Solicitud.SolicitudPlantilla.Plantilla);

        return await new FirmaPreparationService(
                new Mock<IWorkflowDomainService>().Object,
                plantillas.Object,
                Parametros().Object)
            .PrepararDocumentoFirmaAsync(documento, "REF-1", 10, cantidad, pagina, CancellationToken.None);
    }

    private static SolicitudDocumento Documento(bool esFirmante)
    {
        var solicitud = Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
        var plantilla = Plantilla.Crear(6, "Flujo", "Descripcion", false, 10, "GTI", true);
        var enlace = SolicitudPlantilla.Crear(3, solicitud, plantilla);
        enlace.AgregarFirmanteFlujo(11, 12, 1, 1, 10);
        enlace.FirmantesFlujo.Single().AsignarDatosEtapa(1, 10, esFirmante, !esFirmante);
        solicitud.AsignarPlantilla(enlace);
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.Solicitud = solicitud;
        return documento;
    }

    private static Mock<IParametroRepository> Parametros()
    {
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroDetalleDto>());
        parametros.Setup(p => p.ObtenerTiempoProcesoFirmadoArchivoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TiempoProcesoFirmaDto { TiempoArchivo = 2, TiempoMargenSeguridad = 3 });
        parametros.Setup(p => p.ObtenerTamanioFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TamanioFirmaDto
            {
                FirmaAnchoFirmante = 175,
                FirmaAltoFirmante = 40,
                FirmaAnchoVisador = 80,
                FirmaAltoVisador = 25
            });
        parametros.Setup(p => p.ObtenerPosicionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PosicionFirmaDto { PosicionXVisador = 30, PosicionYVisador = 40 });
        parametros.Setup(p => p.ConfiguracionFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionFirmaDto { NombreTag = "[F]", TamanioLetraFirmante = 8, TamanioLetraVisado = 6 });
        return parametros;
    }
}
