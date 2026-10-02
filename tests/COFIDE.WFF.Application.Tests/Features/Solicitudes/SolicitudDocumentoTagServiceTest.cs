using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Features.Solicitudes;

public class SolicitudDocumentoTagServiceTest
{
    [Fact]
    public async Task Crear_Sin_Documentos_No_Debe_Consultar_Parametros()
    {
        var parametros = new Mock<IParametroRepository>();

        await Servicio(parametros).CrearDocumentoTagDesdeSolicitud(Borrador(), "[]", CancellationToken.None);

        parametros.Verify(p => p.ObtenerTamanioFirmaAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Crear_Con_Json_Invalido_Debe_Lanzar_APP_SOL_011()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);

        var act = () => Servicio().CrearDocumentoTagDesdeSolicitud(solicitud, "no-es-json", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_011");
    }

    [Fact]
    public async Task Crear_Debe_Agregar_La_Etiqueta_Al_Documento_Del_Mismo_Nombre()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var parametros = new Mock<IParametroRepository>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        parametros.Setup(p => p.ObtenerTamanioFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TamanioFirmaDto { FirmaAnchoFirmante = 175, FirmaAltoFirmante = 40 });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDocumentoTagAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 50 });

        await new SolicitudDocumentoTagService(correlativos.Object, parametros.Object)
            .CrearDocumentoTagDesdeSolicitud(solicitud, """
                [{"nombreDocumento":"contrato.pdf","coordenadas":[{"numeroPagina":1,"x":10,"y":20}]}]
                """, CancellationToken.None);

        var etiqueta = solicitud.Documentos.Single().DocumentoTags.Single();
        etiqueta.Id.Should().Be(50);
        etiqueta.NumeroPagina.Should().Be(1);
        etiqueta.UbicacionX.Should().Be(10);
        etiqueta.UbicacionY.Should().Be(20);
        etiqueta.AnchoFirma.Should().Be(175);
        etiqueta.AltoFirma.Should().Be(40);
    }

    [Fact]
    public async Task Actualizar_Debe_Agregar_La_Nueva_Y_Retirar_La_Que_No_Viene()
    {
        var solicitud = Borrador();
        solicitud.AgregarDocumento(4, "contrato", "pdf", "originales", "hash",
            null, null, null, null, null, null, null, null, 10);
        var documento = solicitud.Documentos.Single();
        documento.AsignarDocumentoConTag("contrato", "pdf", "tags", "hash-tag");
        documento.AgregarDocumentoTag(8, 1, 1, 1, 175, 40);
        var parametros = new Mock<IParametroRepository>();
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        parametros.Setup(p => p.ObtenerTamanioFirmaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TamanioFirmaDto { FirmaAnchoFirmante = 175, FirmaAltoFirmante = 40 });
        correlativos.Setup(c => c.ObtenerBloqueSiguienteIdDocumentoTagAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<long> { 50 });

        await new SolicitudDocumentoTagService(correlativos.Object, parametros.Object)
            .AntualizarDocumentoTagDesdeSolicitud(solicitud, """
                [{"nombreDocumento":"contrato.pdf","coordenadas":[{"id":0,"numeroPagina":2,"x":5,"y":6}]}]
                """, CancellationToken.None);

        documento.DocumentoTags.Should().ContainSingle(t => t.Id == 8 && t.Estado == false);
        documento.DocumentoTags.Should().ContainSingle(t => t.Id == 50 && t.NumeroPagina == 2 && t.Estado);
    }

    private static SolicitudDocumentoTagService Servicio(Mock<IParametroRepository>? parametros = null) =>
        new(new Mock<IGeneradorCorrelativoIdRepository>().Object, (parametros ?? new Mock<IParametroRepository>()).Object);

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);
}
