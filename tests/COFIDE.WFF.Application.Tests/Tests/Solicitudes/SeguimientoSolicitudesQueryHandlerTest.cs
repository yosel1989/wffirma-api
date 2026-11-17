using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;
using COFIDE.WFF.Application.Tests.Support;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public sealed record SeguimientoUsuarioData(
    long? UserIdLocal, long? UserCargoId, long? UserAreaId, string? CodigoRolUsuario);

public sealed record SeguimientoFiltrosData(
    string? Fechainicio, string? Fechafin,
    List<long>? Gerencia, List<long>? Departamento, List<long>? Solicitante,
    List<long>? Tiposolicitud, List<long>? Subtiposolicitud, List<long>? TipoOperacion,
    string? Asunto, string? Referencia, List<long>? Estado, List<long>? Firmante,
    string? Codigosolicitud, bool? Encurso, bool? Aprobada, bool? Rechazada, bool? IncluirResumen);

public sealed record SeguimientoItemData(long IdSolicitud, string? CodigoSolicitud, string? Estado);

public sealed record SeguimientoResumenData(long Total, long EnCurso, long Aprobado, long Rechazado);

public sealed record SeguimientoRepositorioData(
    int TotalCount, List<SeguimientoItemData> Items, SeguimientoResumenData Resumen);

public sealed record SeguimientoEsperadoData(
    string? CodigoError, int? TotalPages, bool? HasPrevious, bool? HasNext);

public sealed record SeguimientoCase(
    string Caso,
    SeguimientoUsuarioData Usuario,
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    SeguimientoFiltrosData Filtros,
    SeguimientoRepositorioData? Repositorio,
    SeguimientoEsperadoData Esperado)
{
    public override string ToString() => Caso;
}

public class SeguimientoSolicitudesQueryHandlerTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<SeguimientoCase>("Solicitudes/seguimiento-solicitudes.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Handle_Debe_Responder_Segun_El_Caso(SeguimientoCase caso)
    {
        var repositorio = new Mock<ISolicitudSeguimientoRepository>(MockBehavior.Strict);
        var repositorioAccesos = new Mock<IAccesoEspecialRepository>(MockBehavior.Strict);
        if (caso.Repositorio is not null)
        {
            ConfigurarRepositorio(repositorio, caso);
            if (string.Equals(caso.Usuario.CodigoRolUsuario?.Trim(), "COORD", StringComparison.OrdinalIgnoreCase))
                repositorioAccesos.Setup(r => r.GetAreasByUserIdAsync(caso.Usuario.UserIdLocal!.Value))
                    .ReturnsAsync([]);
        }

        var act = () => new SeguimientoSolicitudesQueryHandler(
                Usuario(caso.Usuario), repositorioAccesos.Object, repositorio.Object)
            .Handle(Consulta(caso), CancellationToken.None);

        if (caso.Esperado.CodigoError is not null)
        {
            var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
            ex.Which.Error.Code.Should().Be(caso.Esperado.CodigoError);
            repositorio.VerifyNoOtherCalls();
            repositorioAccesos.VerifyNoOtherCalls();
            return;
        }

        var respuesta = await act();
        var resumen = caso.Repositorio!.Resumen;
        respuesta.Total.Should().Be(resumen.Total);
        respuesta.EnCurso.Should().Be(resumen.EnCurso);
        respuesta.Aprobado.Should().Be(resumen.Aprobado);
        respuesta.Rechazado.Should().Be(resumen.Rechazado);
        respuesta.PageNumber.Should().Be(caso.PageNumber);
        respuesta.PageSize.Should().Be(caso.PageSize);
        respuesta.TotalCount.Should().Be(caso.Repositorio.TotalCount);
        respuesta.TotalPages.Should().Be(caso.Esperado.TotalPages);
        respuesta.HasPrevious.Should().Be(caso.Esperado.HasPrevious!.Value);
        respuesta.HasNext.Should().Be(caso.Esperado.HasNext!.Value);
        respuesta.ListadoSolicitudes.Select(s => s.CodigoSolicitud)
            .Should().Equal(caso.Repositorio.Items.Select(i => i.CodigoSolicitud));

        var filtros = caso.Filtros;
        repositorio.Verify(r => r.ObtenerPaginadoSeguimiento(
            It.Is<PagedQueryParams>(p =>
                p.PageNumber == caso.PageNumber &&
                p.PageSize == caso.PageSize &&
                p.SearchTerm == caso.SearchTerm),
            caso.Usuario.UserIdLocal,
            caso.Usuario.UserAreaId,
            caso.Usuario.CodigoRolUsuario,
            null,
            true,
            caso.Usuario.UserAreaId,
            filtros.Fechainicio, filtros.Fechafin,
            filtros.Gerencia, filtros.Departamento, filtros.Solicitante,
            filtros.Tiposolicitud, filtros.Subtiposolicitud, filtros.TipoOperacion,
            filtros.Asunto, filtros.Referencia, filtros.Estado, filtros.Firmante, filtros.Codigosolicitud,
            filtros.Encurso, filtros.Aprobada, filtros.Rechazada, filtros.IncluirResumen,
            It.IsAny<CancellationToken>()), Times.Once);
        repositorioAccesos.Verify(r => r.GetAreasByUserIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Coordinador_Debe_Pasar_Sus_AreasDeAccesoEspecial_Al_Repositorio()
    {
        var areasAccesoEspecial = new List<long> { 5, 50 };
        var currentUser = Usuario(new SeguimientoUsuarioData(20, 7, 90, "COORD"));
        var repositorioAccesos = new Mock<IAccesoEspecialRepository>(MockBehavior.Strict);
        repositorioAccesos.Setup(r => r.GetAreasByUserIdAsync(20))
            .ReturnsAsync(areasAccesoEspecial);

        var resultadoVacio = new PagedResult<SeguimientoSolicitudesDto, SeguimientoSolicitudesResumenDto>(
            [], 0, 1, 10)
        {
            ExtraData = new SeguimientoSolicitudesResumenDto()
        };
        var repositorio = new Mock<ISolicitudSeguimientoRepository>(MockBehavior.Strict);
        repositorio.Setup(r => r.ObtenerPaginadoSeguimiento(
                It.IsAny<PagedQueryParams>(),
                It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<bool>(), It.IsAny<long?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<string?>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultadoVacio);

        var query = new SeguimientoSolicitudesQuery(
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null);
        var handler = new SeguimientoSolicitudesQueryHandler(
            currentUser, repositorioAccesos.Object, repositorio.Object);

        await handler.Handle(query, CancellationToken.None);

        repositorioAccesos.Verify(r => r.GetAreasByUserIdAsync(20), Times.Once);
        repositorio.Verify(r => r.ObtenerPaginadoSeguimiento(
            It.IsAny<PagedQueryParams>(), 20, 90, "COORD",
            It.Is<List<long>?>(areas => areas != null && areas.SequenceEqual(areasAccesoEspecial)),
            true, 90,
            It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
            It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
            It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<string?>(),
            It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TamanoPagina_Debe_Limitarse_A_50()
    {
        var repositorio = new Mock<ISolicitudSeguimientoRepository>(MockBehavior.Strict);
        var resultado = new PagedResult<SeguimientoSolicitudesDto, SeguimientoSolicitudesResumenDto>(
            [], 0, 1, 50)
        {
            ExtraData = new SeguimientoSolicitudesResumenDto()
        };
        repositorio.Setup(r => r.ObtenerPaginadoSeguimiento(
                It.Is<PagedQueryParams>(p => p.PageSize == 50 && p.PageNumber == 1),
                It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<bool>(), It.IsAny<long?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<string?>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);
        var usuario = Usuario(new SeguimientoUsuarioData(20, 7, 90, "SOLAPR"));
        var accesos = new Mock<IAccesoEspecialRepository>(MockBehavior.Strict);
        var query = new SeguimientoSolicitudesQuery(
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null)
        {
            PageSize = 120
        };
        var handler = new SeguimientoSolicitudesQueryHandler(usuario, accesos.Object, repositorio.Object);

        var respuesta = await handler.Handle(query, CancellationToken.None);

        respuesta.PageSize.Should().Be(50);
        repositorio.Verify(r => r.ObtenerPaginadoSeguimiento(
            It.Is<PagedQueryParams>(p => p.PageSize == 50 && p.PageNumber == 1),
            It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string?>(),
            It.IsAny<List<long>?>(), It.IsAny<bool>(), It.IsAny<long?>(),
            It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
            It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
            It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<string?>(),
            It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
            It.IsAny<CancellationToken>()), Times.Once);
        accesos.VerifyNoOtherCalls();
    }

    private static void ConfigurarRepositorio(Mock<ISolicitudSeguimientoRepository> repositorio, SeguimientoCase caso)
    {
        var datos = caso.Repositorio!;
        var pagina = new PagedResult<SeguimientoSolicitudesDto, SeguimientoSolicitudesResumenDto>(
            datos.Items.Select(i => new SeguimientoSolicitudesDto
            {
                IdSolicitud = i.IdSolicitud,
                CodigoSolicitud = i.CodigoSolicitud,
                Estado = i.Estado
            }),
            datos.TotalCount,
            caso.PageNumber,
            caso.PageSize)
        {
            ExtraData = new SeguimientoSolicitudesResumenDto
            {
                Total = datos.Resumen.Total,
                EnCurso = datos.Resumen.EnCurso,
                Aprobado = datos.Resumen.Aprobado,
                Rechazado = datos.Resumen.Rechazado
            }
        };

        repositorio.Setup(r => r.ObtenerPaginadoSeguimiento(
                It.IsAny<PagedQueryParams>(),
                It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<List<long>?>(), It.IsAny<bool>(), It.IsAny<long?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<string?>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagina);
    }

    private static ICurrentUserService Usuario(SeguimientoUsuarioData datos)
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(datos.UserIdLocal);
        usuario.Setup(u => u.UserCargoId).Returns(datos.UserCargoId);
        usuario.Setup(u => u.UserAreaId).Returns(datos.UserAreaId);
        usuario.Setup(u => u.CodigoRolUsuario).Returns(datos.CodigoRolUsuario);
        return usuario.Object;
    }

    private static SeguimientoSolicitudesQuery Consulta(SeguimientoCase caso)
    {
        var filtros = caso.Filtros;
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
}
