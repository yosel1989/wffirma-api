using COFIDE.WFF.Api.Controllers.DTOs;
using COFIDE.WFF.Api.Controllers.v1;
using COFIDE.WFF.Api.Request.Solicitudes;
using COFIDE.WFF.Api.Tests.Support;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace COFIDE.WFF.Api.Tests.Tests.Solicitudes
{
    public sealed record GestionApiFiltrosData(
        string? Fechainicio, string? Fechafin,
        List<long>? Gerencia, List<long>? Departamento, List<long>? Solicitante,
        List<long>? Firmante, List<long>? Estado, string? Codigosolicitud);

    public sealed record GestionApiItemData(
        long IdSolicitud,
        string? CodigoSolicitud,
        string? Solicitante,
        string? AprobadorActual,
        string? Estado,
        DateTime FechaCreacion,
        DateTime? FechaModificacion);

    public sealed record GestionApiRespuestaData(
        long Total,
        List<GestionApiItemData> Items,
        int PageNumber, int PageSize, int TotalCount);

    public sealed record GestionApiEsperadoData(bool Success, string Message);

    public sealed record GestionApiCase(
        string Caso,
        int PageNumber,
        int PageSize,
        List<SortDto>? Sort,
        GestionApiFiltrosData Consulta,
        GestionApiRespuestaData Respuesta,
        GestionApiEsperadoData Esperado)
    {
        public override string ToString() => Caso;
    }

    public class GestionSolicitudesTest
    {
        public static IEnumerable<object[]> Casos =>
            JsonResource.Cases<GestionApiCase>("Solicitudes/gestion-solicitudes.json");

        [Theory]
        [MemberData(nameof(Casos))]
        public async Task Gestion_Debe_Enviar_La_Consulta_Y_Responder_Ok(GestionApiCase caso)
        {
            var request = Consulta(caso);
            var listado = Respuesta(caso.Respuesta);

            ObtenerGestionSolicitudesQuery? queryEnviada = null;
            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Send(It.IsAny<ObtenerGestionSolicitudesQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ObtenerGestionSolicitudesResponseDto>, CancellationToken>(
                    (q, _) => queryEnviada = (ObtenerGestionSolicitudesQuery)q)
                .ReturnsAsync(listado);

            var controller = new SolicitudController(mediator.Object, new Mock<ICorrelationContext>().Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var actionResult = await controller.GestionSolicitudesPaginado(request);

            var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value.Should().BeOfType<ResponseDto<ObtenerGestionSolicitudesResponseDto>>().Subject;
            body.Success.Should().Be(caso.Esperado.Success);
            body.Message.Should().Be(caso.Esperado.Message);
            body.CorrelationId.Should().Be(controller.HttpContext.TraceIdentifier);
            body.Data!.Total.Should().Be(caso.Respuesta.Total);
            body.Data.PageNumber.Should().Be(caso.Respuesta.PageNumber);
            body.Data.PageSize.Should().Be(caso.Respuesta.PageSize);
            body.Data.TotalCount.Should().Be(caso.Respuesta.TotalCount);
            body.Data.ListadoSolicitudes.Should().HaveCount(caso.Respuesta.Items.Count);
            body.Data.ListadoSolicitudes
                .Select(s => new
                {
                    s.IdSolicitud,
                    s.CodigoSolicitud,
                    s.Solicitante,
                    s.AprobadorActual,
                    s.Estado,
                    s.FechaCreacion,
                    s.FechaModificacion
                })
                .Should().BeEquivalentTo(
                    caso.Respuesta.Items.Select(i => new
                    {
                        i.IdSolicitud,
                        i.CodigoSolicitud,
                        i.Solicitante,
                        i.AprobadorActual,
                        i.Estado,
                        i.FechaCreacion,
                        i.FechaModificacion
                    }),
                    opciones => opciones.WithStrictOrdering());

            mediator.Verify(m => m.Send(
                It.IsAny<ObtenerGestionSolicitudesQuery>(),
                It.IsAny<CancellationToken>()), Times.Once);

            var filtros = caso.Consulta;
            queryEnviada.Should().NotBeNull();
            queryEnviada!.PageNumber.Should().Be(caso.PageNumber);
            queryEnviada.PageSize.Should().Be(caso.PageSize);
            queryEnviada.Fechainicio.Should().Be(filtros.Fechainicio);
            queryEnviada.Fechafin.Should().Be(filtros.Fechafin);
            queryEnviada.Gerencia.Should().BeEquivalentTo(filtros.Gerencia);
            queryEnviada.Departamento.Should().BeEquivalentTo(filtros.Departamento);
            queryEnviada.Solicitante.Should().BeEquivalentTo(filtros.Solicitante);
            queryEnviada.Firmante.Should().BeEquivalentTo(filtros.Firmante);
            queryEnviada.Estado.Should().BeEquivalentTo(filtros.Estado);
            queryEnviada.Codigosolicitud.Should().Be(filtros.Codigosolicitud);
        }

        private static GestionSolicitudesRequest Consulta(GestionApiCase caso)
        {
            var filtros = caso.Consulta;

            return new GestionSolicitudesRequest
            {
                Fechainicio = filtros.Fechainicio,
                Fechafin = filtros.Fechafin,
                Gerencia = filtros.Gerencia,
                Departamento = filtros.Departamento,
                Solicitante = filtros.Solicitante,
                Firmante = filtros.Firmante,
                Estado = filtros.Estado,
                Codigosolicitud = filtros.Codigosolicitud,

                PageNumber = caso.PageNumber,
                PageSize = caso.PageSize,
                Sorts = caso.Sort,
            };
        }

        private static ObtenerGestionSolicitudesResponseDto Respuesta(GestionApiRespuestaData datos) =>
            new()
            {
                Total = datos.Total,
                ListadoSolicitudes = datos.Items.Select(i => new ObtenerGestionSolicitudesDto
                {
                    IdSolicitud = i.IdSolicitud,
                    CodigoSolicitud = i.CodigoSolicitud,
                    Solicitante = i.Solicitante,
                    AprobadorActual = i.AprobadorActual,
                    Estado = i.Estado,
                    FechaCreacion = i.FechaCreacion,
                    FechaModificacion = i.FechaModificacion
                }),
                PageNumber = datos.PageNumber,
                PageSize = datos.PageSize,
                TotalCount = datos.TotalCount
            };
    }
}