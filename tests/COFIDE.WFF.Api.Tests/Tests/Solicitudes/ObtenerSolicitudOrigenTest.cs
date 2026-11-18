using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarTodoSolicitudZip;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Solicitudes;

public class ObtenerSolicitudOrigenTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("mis_solicitudes")]
    [InlineData("seguimiento_solicitudes")]
    [InlineData("bandeja_entrada")]
    public async Task GetSolicitud_Debe_Enviar_Origen_Al_Query(string? origen)
    {
        using var cancelacion = new CancellationTokenSource();
        var dto = new SolicitudDto();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(
                It.Is<ObtenerSolicitudQuery>(q => q.IdSolicitud == 140 && !q.EsReuso && q.Origen == origen),
                cancelacion.Token))
            .ReturnsAsync(dto);
        var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object);

        var resultado = await controller.GetSolicitud(140, origen: origen, cancellationToken: cancelacion.Token);

        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ResponseDto<SolicitudDto>>()
            .Which.Data.Should().BeSameAs(dto);
        mediator.Verify(m => m.Send(
            It.Is<ObtenerSolicitudQuery>(q => q.IdSolicitud == 140 && !q.EsReuso && q.Origen == origen),
            cancelacion.Token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ObtenerDocumento_Debe_Enviar_Origen_Al_Query()
    {
        using var cancelacion = new CancellationTokenSource();
        var dto = new DescargarSolicitudDocumentoDto();
        const string origen = "mis_solicitudes";
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(
                It.Is<DescargarSolicitudDocumentoQuery>(q =>
                    q.IdSolicitudDocumento == 4 && q.Origen == origen),
                cancelacion.Token))
            .ReturnsAsync(dto);
        var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object);

        var resultado = await controller.ObtenerDocumento(4, origen, cancelacion.Token);

        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ResponseDto<DescargarSolicitudDocumentoDto>>()
            .Which.Data.Should().BeSameAs(dto);
        mediator.Verify(m => m.Send(
            It.Is<DescargarSolicitudDocumentoQuery>(q =>
                q.IdSolicitudDocumento == 4 && q.Origen == origen),
            cancelacion.Token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DescargarZip_Debe_Enviar_Origen_Al_Query()
    {
        using var cancelacion = new CancellationTokenSource();
        const string origen = "seguimiento_solicitudes";
        var dto = new DescargarTodoSolicitudZipDto
        {
            Archivo = [1],
            ContentType = "application/zip",
            NombreArchivo = "solicitud.zip"
        };
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(m => m.Send(
                It.Is<DescargarTodoSolicitudZipQuery>(q => q.Id == 12 && q.Origen == origen),
                cancelacion.Token))
            .ReturnsAsync(dto);
        var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object);

        var resultado = await controller.DescargarZip(12, origen, cancelacion.Token);

        var archivo = resultado.Should().BeOfType<FileContentResult>().Subject;
        archivo.FileDownloadName.Should().Be("solicitud.zip");
        archivo.ContentType.Should().Be("application/zip");
        archivo.FileContents.Should().Equal(1);
        mediator.Verify(m => m.Send(
            It.Is<DescargarTodoSolicitudZipQuery>(q => q.Id == 12 && q.Origen == origen),
            cancelacion.Token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }
}
