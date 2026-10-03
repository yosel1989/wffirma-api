using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Application.Tests.Support;
using COFIDE.WFF.Domain.Common.Pageds;
using FluentAssertions;
using Moq;
using applicationException = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Tests.Solicitudes;

#region Casos de prueba

public sealed record UsuarioActualDataObtener(
    long? UserIdLocal, long? UserCargoId, long? UserAreaId, string? CodigoRolUsuario);

public sealed record FiltrosData(
    string? FechaInicio, string? FechaFin,
    List<long>? Gerencia, List<long>? Departamento, List<long>? Solicitante,
    List<long>? TipoSolicitud, List<long>? SubTipoSolicitud, List<long>? TipoOperacion,
    List<long>? AprobadorActual, List<long>? Estado,
    string? CodigoSolicitud, string? Asunto, string? Referencia,
    bool? Urgente, bool? Usuario, bool? UsuarioDepartamento, bool? IncluirResumen);

public sealed record ResumenData(long MisSolicitudes, long Total, long MiDepartamento, long Urgente);

public sealed record RepositorioData(int TotalCount, List<string> CodigosItems, ResumenData Resumen);

public sealed record EsperadoData(
    int PageSizeEnviadoAlRepositorio, int TotalPages,
    bool HasPrevious, bool HasNext, int ItemsEnPagina);

public sealed record PaginadoCase(
    string Caso,
    UsuarioActualDataObtener Usuario,
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    FiltrosData Filtros,
    RepositorioData Repositorio,
    EsperadoData Esperado)
{
    public override string ToString() => Caso;
}

#endregion

public class ObtenerSolicitudPaginadoQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new(MockBehavior.Loose);
    private readonly Mock<ISolicitudQueryRepository> _solicitudRepository = new(MockBehavior.Strict);

    /// <summary>Capturado desde el Setup para poder afirmar con mensajes legibles.</summary>
    private PagedQueryParams? _paramsRecibidos;

    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<PaginadoCase>("Solicitudes/obtener-solicitud-paginado.json");

    #region Camino feliz

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Handle_Debe_Traducir_La_Paginacion_Al_Puerto(PaginadoCase caso)
    {
        ConfigurarUsuario(caso.Usuario);
        ConfigurarRepositorio(caso);

        await CrearHandler().Handle(CrearQuery(caso), CancellationToken.None);

        _paramsRecibidos.Should().NotBeNull();
        _paramsRecibidos!.PageNumber.Should().Be(caso.PageNumber);
        _paramsRecibidos.PageSize.Should().Be(caso.Esperado.PageSizeEnviadoAlRepositorio);
        _paramsRecibidos.SearchTerm.Should().Be(caso.SearchTerm);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Handle_Debe_Proyectar_El_Resumen_Y_Los_Metadatos_En_La_Respuesta(PaginadoCase caso)
    {
        ConfigurarUsuario(caso.Usuario);
        ConfigurarRepositorio(caso);

        var response = await CrearHandler().Handle(CrearQuery(caso), CancellationToken.None);

        response.MisSolicitudes.Should().Be(caso.Repositorio.Resumen.MisSolicitudes);
        response.Total.Should().Be(caso.Repositorio.Resumen.Total);
        response.MiDepartamento.Should().Be(caso.Repositorio.Resumen.MiDepartamento);
        response.Urgente.Should().Be(caso.Repositorio.Resumen.Urgente);

        response.TotalCount.Should().Be(caso.Repositorio.TotalCount);
        response.PageNumber.Should().Be(caso.PageNumber);
        response.PageSize.Should().Be(caso.Esperado.PageSizeEnviadoAlRepositorio);

        response.TotalPages.Should().Be(caso.Esperado.TotalPages);
        response.HasPrevious.Should().Be(caso.Esperado.HasPrevious);
        response.HasNext.Should().Be(caso.Esperado.HasNext);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Handle_Debe_Devolver_Los_Items_Del_Repositorio_Sin_Transformarlos(PaginadoCase caso)
    {
        ConfigurarUsuario(caso.Usuario);
        ConfigurarRepositorio(caso);

        var response = await CrearHandler().Handle(CrearQuery(caso), CancellationToken.None);

        response.ListadoSolicitudItem.Should().HaveCount(caso.Esperado.ItemsEnPagina);
        response.ListadoSolicitudItem.Select(i => i.CodigoSolicitud)
            .Should().Equal(caso.Repositorio.CodigosItems);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task Handle_Debe_Reenviar_El_Contexto_Del_Usuario_Y_Todos_Los_Filtros(PaginadoCase caso)
    {
        ConfigurarUsuario(caso.Usuario);
        ConfigurarRepositorio(caso);

        var query = CrearQuery(caso);
        using var cts = new CancellationTokenSource();

        await CrearHandler().Handle(query, cts.Token);

        // Blinda las 23 posiciones del puerto: si alguien reordena argumentos, esto falla.
        _solicitudRepository.Verify(r => r.ObtenerPaginado(
            It.IsAny<PagedQueryParams>(),
            caso.Usuario.UserIdLocal,
            caso.Usuario.UserAreaId,
            caso.Usuario.CodigoRolUsuario,
            caso.Usuario.UserAreaId,      // el handler reusa el area como departamento
            query.Fechainicio,
            query.Fechafin,
            query.Gerencia,
            query.Departamento,
            query.Solicitante,
            query.Tiposolicitud,
            query.Subtiposolicitud,
            query.TipoOperacion,
            query.AprobadorActual,
            query.Estado,
            query.Codigosolicitud,
            query.Asunto,
            query.Referencia,
            query.Urgente,
            query.Usuario,
            query.Usuariodepartamento,
            query.IncluirResumen,
            cts.Token),
            Times.Once);

        _solicitudRepository.VerifyNoOtherCalls();
    }

    #endregion

    #region Autenticación

    [Fact]
    public async Task Handle_Sin_UserIdLocal_Debe_Lanzar_AUTH_REQUIRED()
    {
        ConfigurarUsuario(new UsuarioActualDataObtener(null, 4, 7, "COORDINADOR"));

        var act = () => CrearHandler().Handle(QueryMinimo(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<applicationException.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UserNotAuthenticated.Code);

        // No debe tocar la base si el usuario no es válido.
        _solicitudRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Sin_UserCargoId_Debe_Lanzar_AUTH_REQUIRED()
    {
        ConfigurarUsuario(new UsuarioActualDataObtener(99, null, 7, "COORDINADOR"));

        var act = () => CrearHandler().Handle(QueryMinimo(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<applicationException.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UserNotAuthenticated.Code);

        _solicitudRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Sin_Area_Debe_Continuar_Y_Enviar_Nulos_Al_Puerto()
    {
        // El handler no exige area: documenta que la consulta se ejecuta sin filtro de departamento.
        ConfigurarUsuario(new UsuarioActualDataObtener(99, 4, null, "ADMINISTRADOR"));
        ConfigurarRepositorioVacio();

        var act = () => CrearHandler().Handle(QueryMinimo(), CancellationToken.None);

        await act.Should().NotThrowAsync();

        _paramsRecibidos.Should().NotBeNull();
    }

    #endregion

    #region Contratos frágiles

    [Fact]
    public async Task Handle_Debe_Usar_CodigoRolUsuario_Y_No_UserCodigoRolUsuario()
    {
        // La interfaz expone dos propiedades casi idénticas; esta fija cuál consume el handler.
        _currentUser.Setup(u => u.UserIdLocal).Returns(99);
        _currentUser.Setup(u => u.UserCargoId).Returns(4);
        _currentUser.Setup(u => u.UserAreaId).Returns(7);
        _currentUser.Setup(u => u.CodigoRolUsuario).Returns("ROL-CORRECTO");
        _currentUser.Setup(u => u.UserCodigoRolUsuario).Returns("ROL-INCORRECTO");

        ConfigurarRepositorioVacio();

        await CrearHandler().Handle(QueryMinimo(), CancellationToken.None);

        _solicitudRepository.Verify(r => r.ObtenerPaginado(
            It.IsAny<PagedQueryParams>(), It.IsAny<long?>(), It.IsAny<long?>(),
            "ROL-CORRECTO",
            It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
            It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
            It.IsAny<List<long>>(), It.IsAny<List<long>>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Con_PageSize_Cero_Hoy_Lo_Reenvia_Sin_Normalizar()
    {
        ConfigurarUsuario(new UsuarioActualDataObtener(99, 4, 7, "COORDINADOR"));
        ConfigurarRepositorioVacio();

        var query = QueryMinimo();
        query.PageNumber = 0;
        query.PageSize = 0;

        await CrearHandler().Handle(query, CancellationToken.None);

        // El handler nunca llama a PagedQueryParams.Normalize(), así que los ceros llegan al repositorio.
        _paramsRecibidos!.PageNumber.Should().Be(0);
        _paramsRecibidos.PageSize.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Cuando_El_Repositorio_No_Devuelve_Resumen_Hoy_Falla()
    {
        ConfigurarUsuario(new UsuarioActualDataObtener(99, 4, 7, "COORDINADOR"));

        // ExtraData null es posible cuando IncluirResumen es false.
        var sinResumen = new PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>(
            [], totalCount: 0, pageNumber: 1, pageSize: 10)
        {
            ExtraData = null
        };

        ConfigurarRepositorio(sinResumen);

        var query = QueryMinimo() with { IncluirResumen = false };

        var act = () => CrearHandler().Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Handle_Debe_Propagar_La_Cancelacion_Del_Repositorio()
    {
        ConfigurarUsuario(new UsuarioActualDataObtener(99, 4, 7, "COORDINADOR"));

        _solicitudRepository
            .Setup(r => r.ObtenerPaginado(
                It.IsAny<PagedQueryParams>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string>(),
                It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => CrearHandler().Handle(QueryMinimo(), CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Helpers

    private ObtenerSolicitudPaginadoQueryHandler CrearHandler() =>
        new(_currentUser.Object, _solicitudRepository.Object);

    private void ConfigurarUsuario(UsuarioActualDataObtener usuario)
    {
        _currentUser.Setup(u => u.UserIdLocal).Returns(usuario.UserIdLocal);
        _currentUser.Setup(u => u.UserCargoId).Returns(usuario.UserCargoId);
        _currentUser.Setup(u => u.UserAreaId).Returns(usuario.UserAreaId);
        _currentUser.Setup(u => u.CodigoRolUsuario).Returns(usuario.CodigoRolUsuario);
    }

    private void ConfigurarRepositorio(PaginadoCase caso)
    {
        var items = caso.Repositorio.CodigosItems
            .Select((codigo, indice) => new SolicitudPaginadoDto
            {
                IdSolicitud = indice + 1,
                CodigoSolicitud = codigo,
                Estado = "En Curso"
            })
            .ToList();

        var resultado = new PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>(
            items,
            caso.Repositorio.TotalCount,
            caso.PageNumber,
            caso.Esperado.PageSizeEnviadoAlRepositorio)
        {
            ExtraData = new SolicitudResumenPaginadoDto
            {
                MisSolicitudes = caso.Repositorio.Resumen.MisSolicitudes,
                Total = caso.Repositorio.Resumen.Total,
                MiDepartamento = caso.Repositorio.Resumen.MiDepartamento,
                Urgente = caso.Repositorio.Resumen.Urgente
            }
        };

        ConfigurarRepositorio(resultado);
    }

    private void ConfigurarRepositorioVacio() =>
        ConfigurarRepositorio(
            new PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>(
                [], totalCount: 0, pageNumber: 1, pageSize: 10)
            {
                ExtraData = new SolicitudResumenPaginadoDto()
            });

    private void ConfigurarRepositorio(
        PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto> resultado) =>
        _solicitudRepository
            .Setup(r => r.ObtenerPaginado(
                It.Is<PagedQueryParams>(p => CapturarParams(p)),
                It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<long?>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado);

    /// <summary>
    /// El puerto tiene 23 parámetros, más de los 16 que admite Callback de Moq.
    /// Capturar desde el matcher permite afirmar con mensajes legibles.
    /// </summary>
    private bool CapturarParams(PagedQueryParams p)
    {
        _paramsRecibidos = p;
        return true;
    }

    private static ObtenerSolicitudPaginadoQuery CrearQuery(PaginadoCase caso)
    {
        var f = caso.Filtros;

        return new ObtenerSolicitudPaginadoQuery(
            Fechainicio: f.FechaInicio,
            Fechafin: f.FechaFin,
            Gerencia: f.Gerencia,
            Departamento: f.Departamento,
            Solicitante: f.Solicitante,
            Tiposolicitud: f.TipoSolicitud,
            Subtiposolicitud: f.SubTipoSolicitud,
            TipoOperacion: f.TipoOperacion,
            AprobadorActual: f.AprobadorActual,
            Estado: f.Estado,
            Codigosolicitud: f.CodigoSolicitud,
            Asunto: f.Asunto,
            Referencia: f.Referencia,
            Urgente: f.Urgente,
            Usuario: f.Usuario,
            Usuariodepartamento: f.UsuarioDepartamento,
            IncluirResumen: f.IncluirResumen)
        {
            PageNumber = caso.PageNumber,
            PageSize = caso.PageSize,
            SearchTerm = caso.SearchTerm
        };
    }

    private static ObtenerSolicitudPaginadoQuery QueryMinimo() =>
        new(null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, true)
        {
            PageNumber = 1,
            PageSize = 10
        };

    #endregion
}