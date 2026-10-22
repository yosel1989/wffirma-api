using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes;
using COFIDE.WFF.Application.Tests.Solicitudes;
using COFIDE.WFF.Application.Tests.Support;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Tests.Solicitudes
{
    public sealed record GestionFiltrosData(
        string? Fechainicio, string? Fechafin,
        List<long>? Gerencia, List<long>? Departamento, List<long>? Solicitante,
        List<long>? Estado, List<long>? Firmante, string? Codigosolicitud);

    public sealed record GestionItemData(
        long IdSolicitud,
        string CodigoSolicitud,
        string? Solicitante,
        string? AprobadorActual,
        string Estado,
        DateTime FechaCreacion,
        DateTime? FechaModificacion);

    public sealed record GestionRepositorioData(
        int TotalCount, List<GestionItemData> Items);

    public sealed record GestionEsperadoPaginaData(
        long Total,
        int ListadoSolicitudesCount,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasPrevious,
        bool HasNext);

    public sealed record GestionEsperadoData(
        bool Success, string? Code, string? Message, GestionEsperadoPaginaData? Data);

    public sealed record GestionCase(
        string Caso,
        SeguimientoUsuarioData Usuario,
        int PageNumber,
        int PageSize,
        string? SearchTerm,
        GestionFiltrosData? Filtros,
        GestionRepositorioData? Repositorio,
        GestionEsperadoData Esperado)
    {
        public override string ToString() => Caso;
    }

    public class ObtenerGestionSolicitudesQueryHandlerTest
    {
        private const string Archivo = "Solicitudes/gestion-solicitudes.json";

        public static IEnumerable<object[]> CasosExito =>
            JsonResource.Cases<GestionCase>(Archivo)
                .Where(c => ((GestionCase)c[0]).Esperado.Success);

        public static IEnumerable<object[]> CasosError =>
            JsonResource.Cases<GestionCase>(Archivo)
                .Where(c => !((GestionCase)c[0]).Esperado.Success);

        [Theory]
        [MemberData(nameof(CasosExito))]
        public async Task Handle_Debe_Responder_Segun_El_Caso(GestionCase caso)
        {
            var repositorio = new Mock<ISolicitudGestionRepository>(MockBehavior.Strict);
            ConfigurarRepositorio(repositorio, caso);

            var respuesta = await new ObtenerGestionSolicitudesQueryHandler(Usuario(caso.Usuario), repositorio.Object)
                .Handle(Consulta(caso), CancellationToken.None);

            var datos = caso.Repositorio!;
            var esperado = caso.Esperado.Data!;

            respuesta.Total.Should().Be(esperado.Total);
            respuesta.PageNumber.Should().Be(esperado.PageNumber);
            respuesta.PageSize.Should().Be(esperado.PageSize);
            respuesta.TotalCount.Should().Be(esperado.TotalCount);
            respuesta.TotalPages.Should().Be(esperado.TotalPages);
            respuesta.HasPrevious.Should().Be(esperado.HasPrevious);
            respuesta.HasNext.Should().Be(esperado.HasNext);
            respuesta.ListadoSolicitudes.Should().HaveCount(esperado.ListadoSolicitudesCount);
            respuesta.ListadoSolicitudes
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
                    datos.Items.Select(i => new
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

            var filtros = caso.Filtros!;
            repositorio.Verify(r => r.ObtenerPaginadoGestionSeguimiento(
                It.Is<PagedQueryParams>(p =>
                    p.PageNumber == caso.PageNumber &&
                    p.PageSize == caso.PageSize &&
                    p.SearchTerm == caso.SearchTerm),
                caso.Usuario.UserIdLocal,
                caso.Usuario.UserAreaId,
                caso.Usuario.CodigoRolUsuario,
                caso.Usuario.UserAreaId,
                filtros.Fechainicio, filtros.Fechafin,
                filtros.Gerencia, filtros.Departamento, filtros.Solicitante,
                filtros.Estado, filtros.Firmante,
                filtros.Codigosolicitud,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(CasosError))]
        public async Task Handle_Debe_Lanzar_Error_Segun_El_Caso(GestionCase caso)
        {
            var repositorio = new Mock<ISolicitudGestionRepository>(MockBehavior.Strict);

            var act = () => new ObtenerGestionSolicitudesQueryHandler(Usuario(caso.Usuario), repositorio.Object)
                .Handle(Consulta(caso), CancellationToken.None);

            var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
            ex.Which.Error.Code.Should().Be(caso.Esperado.Code);
            repositorio.VerifyNoOtherCalls();
        }

        private static void ConfigurarRepositorio(Mock<ISolicitudGestionRepository> repositorio, GestionCase caso)
        {
            var datos = caso.Repositorio!;
            var pagina = new PagedResult<ObtenerGestionSolicitudesDto, GestionSolicitudesResumenDto>(
                datos.Items.Select(i => new ObtenerGestionSolicitudesDto
                {
                    IdSolicitud = i.IdSolicitud,
                    CodigoSolicitud = i.CodigoSolicitud,
                    Solicitante = i.Solicitante,
                    AprobadorActual = i.AprobadorActual,
                    Estado = i.Estado,
                    FechaCreacion = i.FechaCreacion,
                    FechaModificacion = i.FechaModificacion
                }),
                datos.TotalCount,
                caso.PageNumber,
                caso.PageSize)
            {
                ExtraData = new GestionSolicitudesResumenDto { Total = datos.TotalCount }
            };

            repositorio.Setup(r => r.ObtenerPaginadoGestionSeguimiento(
                    It.IsAny<PagedQueryParams>(),
                    It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<long?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<List<long>?>(), It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                    It.IsAny<List<long>?>(), It.IsAny<List<long>?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagina);
        }

        private static ICurrentUserService Usuario(SeguimientoUsuarioData datos)
        {
            var usuario = new Mock<ICurrentUserService>(MockBehavior.Strict);
            usuario.Setup(u => u.UserIdLocal).Returns(datos.UserIdLocal);
            usuario.Setup(u => u.UserCargoId).Returns(datos.UserCargoId);
            usuario.Setup(u => u.UserAreaId).Returns(datos.UserAreaId);
            usuario.Setup(u => u.CodigoRolUsuario).Returns(datos.CodigoRolUsuario);
            return usuario.Object;
        }

        private static ObtenerGestionSolicitudesQuery Consulta(GestionCase caso)
        {
            var filtros = caso.Filtros
                ?? new GestionFiltrosData(null, null, null, null, null, null, null, null);

            return new ObtenerGestionSolicitudesQuery(
                Fechainicio: filtros.Fechainicio,
                Fechafin: filtros.Fechafin,
                Gerencia: filtros.Gerencia,
                Departamento: filtros.Departamento,
                Solicitante: filtros.Solicitante,
                Estado: filtros.Estado,
                Firmante: filtros.Firmante,
                Codigosolicitud: filtros.Codigosolicitud
            )
            {
                PageNumber = caso.PageNumber,
                PageSize = caso.PageSize,
                SearchTerm = caso.SearchTerm
            };
        }
    }
}