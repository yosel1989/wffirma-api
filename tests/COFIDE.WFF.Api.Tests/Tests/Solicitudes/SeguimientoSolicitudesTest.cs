using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Api.Tests.Support;
using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Solicitudes;

public sealed record SeguimientoApiFiltrosData(
    string? Fechainicio, string? Fechafin,
    List<long>? Gerencia, List<long>? Departamento, List<long>? Solicitante,
    List<long>? Tiposolicitud, List<long>? Subtiposolicitud, List<long>? TipoOperacion,
    string? Asunto, string? Referencia, List<long>? Estado, List<long>? Firmante,
    string? Codigosolicitud, bool? Encurso, bool? Aprobada, bool? Rechazada, bool? IncluirResumen);

public sealed record SeguimientoApiItemData(long IdSolicitud, string? CodigoSolicitud, string? Estado);

public sealed record SeguimientoApiRespuestaData(
    long Total, long EnCurso, long Aprobado, long Rechazado,
    List<SeguimientoApiItemData> Items,
    int PageNumber, int PageSize, int TotalCount);

public sealed record SeguimientoApiEsperadoData(bool Success, string Message);

public sealed record SeguimientoApiCase(
    string Caso,
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    SeguimientoApiFiltrosData Consulta,
    SeguimientoApiRespuestaData Respuesta,
    SeguimientoApiEsperadoData Esperado)
{
    public override string ToString() => Caso;
}

public class SeguimientoSolicitudesTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<SeguimientoApiCase>("Solicitudes/seguimiento-solicitudes.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Seguimiento_Debe_Enviar_La_Consulta_Y_Responder_Ok(SeguimientoApiCase caso)
    {
        var consulta = Consulta(caso);
        var listado = Respuesta(caso.Respuesta);
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(consulta, It.IsAny<CancellationToken>())).ReturnsAsync(listado);
        var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var actionResult = await controller.SeguimientoSolicitudesPaginado(consulta);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ResponseDto<SeguimientoSolicitudesResponseDto>>().Subject;
        body.Success.Should().Be(caso.Esperado.Success);
        body.Message.Should().Be(caso.Esperado.Message);
        body.CorrelationId.Should().Be(controller.HttpContext.TraceIdentifier);
        body.Data!.Total.Should().Be(caso.Respuesta.Total);
        body.Data.EnCurso.Should().Be(caso.Respuesta.EnCurso);
        body.Data.Aprobado.Should().Be(caso.Respuesta.Aprobado);
        body.Data.Rechazado.Should().Be(caso.Respuesta.Rechazado);
        body.Data.PageNumber.Should().Be(caso.Respuesta.PageNumber);
        body.Data.TotalCount.Should().Be(caso.Respuesta.TotalCount);
        body.Data.ListadoSolicitudes.Select(s => s.CodigoSolicitud)
            .Should().Equal(caso.Respuesta.Items.Select(i => i.CodigoSolicitud));
        mediator.Verify(m => m.Send(
            It.Is<SeguimientoSolicitudesQuery>(q =>
                q.Codigosolicitud == caso.Consulta.Codigosolicitud &&
                q.PageNumber == caso.PageNumber &&
                q.IncluirResumen == caso.Consulta.IncluirResumen),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SeguimientoSolicitudesQuery Consulta(SeguimientoApiCase caso)
    {
        var filtros = caso.Consulta;
        return new SeguimientoSolicitudesQuery(
            filtros.Fechainicio, filtros.Fechafin,
            filtros.Gerencia, filtros.Departamento, filtros.Solicitante,
            filtros.Tiposolicitud, filtros.Subtiposolicitud, filtros.TipoOperacion,
            filtros.Asunto, filtros.Referencia, filtros.Estado, filtros.Firmante, filtros.Codigosolicitud,
            filtros.Encurso, filtros.Aprobada, filtros.Rechazada, filtros.IncluirResumen)
        {
            PageNumber = caso.PageNumber,
            PageSize = caso.PageSize,
            SearchTerm = caso.SearchTerm
        };
    }

    private static SeguimientoSolicitudesResponseDto Respuesta(SeguimientoApiRespuestaData datos) =>
        new()
        {
            Total = datos.Total,
            EnCurso = datos.EnCurso,
            Aprobado = datos.Aprobado,
            Rechazado = datos.Rechazado,
            ListadoSolicitudes = datos.Items.Select(i => new SeguimientoSolicitudesDto
            {
                IdSolicitud = i.IdSolicitud,
                CodigoSolicitud = i.CodigoSolicitud,
                Estado = i.Estado
            }),
            PageNumber = datos.PageNumber,
            PageSize = datos.PageSize,
            TotalCount = datos.TotalCount
        };
}
