using System.Reflection;
using AutoMapper;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.DTOs;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.EventHandlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DetectarConflictosCodigosReferencia;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.PermisoSolicitud;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

using COFIDE.WFF.Application.Features.Usuarios.Models;
namespace COFIDE.WFF.Application.Tests.Tests.Consultas;

public class ConsultasRestantesHandlerTests
{
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task Contexto_Sin_Usuario_Local_Debe_Lanzar_AUTH_REQUIRED()
    {
        var usuario = UsuarioActual();
        usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => Contexto(usuario).Handle(new ObtenerContextoUsuarioQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("AUTH_REQUIRED");
    }

    [Fact]
    public async Task Contexto_Sin_Codigo_Debe_Lanzar_AUTH_REQUIRED()
    {
        var usuario = UsuarioActual(10, null);

        var act = () => Contexto(usuario).Handle(new ObtenerContextoUsuarioQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("AUTH_REQUIRED");
    }

    [Fact]
    public async Task Contexto_Sin_Usuario_En_Suc_Debe_Lanzar_USUARIO_NO_VALIDO_SUC()
    {
        var auth = new Mock<IAutenticacionExternaPort>();
        auth.Setup(a => a.AutenticarAsync("USR01", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioAutenticado)null!);

        var act = () => Contexto(UsuarioActual(), auth).Handle(new ObtenerContextoUsuarioQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("USUARIO_NO_VALIDO_SUC");
    }

    [Fact]
    public async Task Contexto_Sin_Usuario_Local_En_Repositorio_Debe_Lanzar_FORBIDDEN()
    {
        var auth = new Mock<IAutenticacionExternaPort>();
        var repo = new Mock<IUsuarioRepository>();
        auth.Setup(a => a.AutenticarAsync("USR01", It.IsAny<CancellationToken>())).ReturnsAsync(Suc());
        repo.Setup(r => r.GetByCuentaLogueoAsync("USR01", false)).ReturnsAsync((Usuario?)null);

        var act = () => Contexto(UsuarioActual(), auth, repo).Handle(new ObtenerContextoUsuarioQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("FORBIDDEN");
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData('G', null, 4L)]
    [InlineData('D', 4L, 9L)]
    public async Task Contexto_Debe_Resolver_Gerencia_Y_Departamento(char? tipo, long? departamento, long? gerencia)
    {
        var auth = new Mock<IAutenticacionExternaPort>();
        var repo = new Mock<IUsuarioRepository>();
        var local = new Usuario(10, RolUsuario.SolicitanteAprobador);
        if (tipo is not null)
        {
            var area = new Area();
            Asignar(area, typeof(BaseEntity<long>), "Id", 4L);
            Asignar(area, typeof(Area), "Tipo", tipo.Value);
            if (tipo == 'D')
            {
                var padre = new Area();
                Asignar(padre, typeof(BaseEntity<long>), "Id", 9L);
                Asignar(area, typeof(Area), "AreaPadre", padre);
            }

            Asignar(local, typeof(Usuario), "Area", area);
        }

        auth.Setup(a => a.AutenticarAsync("USR01", It.IsAny<CancellationToken>())).ReturnsAsync(Suc());
        repo.Setup(r => r.GetByCuentaLogueoAsync("USR01", false)).ReturnsAsync(local);

        var dto = await Contexto(UsuarioActual(), auth, repo).Handle(new ObtenerContextoUsuarioQuery(), CancellationToken.None);

        dto!.CodigoUsuario.Should().Be("USR01");
        dto.DepartamentoId.Should().Be(departamento);
        dto.GerenciaId.Should().Be(gerencia);
        dto.LstMenu.Single().Descripcion.Should().BeEmpty();
        dto.LstSubMenu.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectarCodigo_Sin_Codigos_Debe_Responder_Cero()
    {
        var parametros = new Mock<IParametroRepository>();

        var respuesta = await Detectar(parametros).Handle(new DetectarCodigoQuery(8, []), CancellationToken.None);

        respuesta.CantidadNoAñadida.Should().Be(0);
        parametros.Verify(p => p.ObtenerDetallePorCodigoParametro(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DetectarCodigo_Invalido_No_Debe_Consultar_La_Base()
    {
        var solicitudes = new Mock<ISolicitudQueryRepository>();
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("COD_REFERENCIA", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ParametroDetalleDto { Valor = "3" }]);

        var respuesta = await Detectar(parametros, solicitudes)
            .Handle(new DetectarCodigoQuery(8, ["   ", "ABCD"]), CancellationToken.None);

        respuesta.CantidadNoAñadida.Should().Be(2);
        solicitudes.Verify(s => s.ObtenerCodigosExistentesAsync(
            It.IsAny<long?>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DetectarCodigo_Duplicado_Y_Existente_Debe_Reportar_Ambos()
    {
        var solicitudes = new Mock<ISolicitudQueryRepository>();
        var parametros = new Mock<IParametroRepository>();
        parametros.Setup(p => p.ObtenerDetallePorCodigoParametro("COD_REFERENCIA", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        solicitudes.Setup(s => s.ObtenerCodigosExistentesAsync(8, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CodigoReferenciaExistente("REF-1", "GTI-SOL-00000001-2026")]);

        var respuesta = await Detectar(parametros, solicitudes)
            .Handle(new DetectarCodigoQuery(8, ["REF-1", "REF-1"]), CancellationToken.None);

        respuesta.Conflictos.Should().Contain(c => c.Codigo == "REF-1" && c.Motivo == "Duplicidad");
        respuesta.Conflictos.Should().Contain(c => c.Motivo.Contains("GTI-SOL-00000001-2026"));
        respuesta.CantidadNoAñadida.Should().Be(2);
    }

    [Fact]
    public async Task Permiso_Sin_Solicitud_Debe_Lanzar_APP_SOL_001()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Solicitud?)null);

        var act = () => Permiso(solicitudes).Handle(new PermisoSolicitudQuery(8, false), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_001");
    }

    [Fact]
    public async Task Permiso_Sin_Plantilla_Debe_Lanzar_APP_SOL_005()
    {
        var solicitud = Borrador();
        var asignada = SolicitudPlantilla.Crear(1, solicitud, Plantilla.Crear(5, "Flujo", "Descripcion", false, 10, "GTI", true));
        asignada.ActualizarPlantilla(5);
        solicitud.AsignarPlantilla(asignada);
        var solicitudes = new Mock<ISolicitudRepository>();
        var plantillas = new Mock<IPlantillaRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(solicitud);
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(5, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync((Plantilla?)null);

        var act = () => Permiso(solicitudes, plantillas).Handle(new PermisoSolicitudQuery(8, false), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_005");
    }

    [Fact]
    public async Task Permiso_Del_Solicitante_Debe_Responder_True()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(Borrador());

        var permitido = await Permiso(solicitudes).Handle(new PermisoSolicitudQuery(8, false), CancellationToken.None);

        permitido.Should().BeTrue();
    }

    [Fact]
    public async Task Permiso_De_Otro_Solicitante_Debe_Lanzar_DomainException()
    {
        var solicitudes = new Mock<ISolicitudRepository>();
        solicitudes.Setup(s => s.ObtenerAgregadoSolicitudAsync(8, It.IsAny<CancellationToken>(), true)).ReturnsAsync(Borrador());
        var usuario = UsuarioActual(99, "USR99");

        var act = () => Permiso(solicitudes, usuario: usuario).Handle(new PermisoSolicitudQuery(8, false), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Documentos_Bloqueados_Debe_Proyectar_La_Pagina()
    {
        var repo = new Mock<ISolicitudDocumentoRepository>();
        var item = new DocumentoBloqueadoPaginadoDto { NombreArchivo = "contrato.pdf" };
        repo.Setup(r => r.ObtenerGestionDocumentosBloqueadosPaginado(
                It.IsAny<PagedQueryParams>(), "2026-01-01", null, "contrato", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<DocumentoBloqueadoPaginadoDto>([item], 12, 2, 10));
        var query = new ObtenerDocumentoBloqueadoPaginadoQuery("2026-01-01", null, "contrato", null, null)
        {
            PageNumber = 2,
            PageSize = 10
        };

        var pagina = await new ObtenerDocumentoBloqueadoPaginadoQueryHandler(repo.Object)
            .Handle(query, CancellationToken.None);

        pagina.TotalCount.Should().Be(12);
        pagina.PageNumber.Should().Be(2);
        pagina.Items.Single().NombreArchivo.Should().Be("contrato.pdf");
        pagina.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task Excel_Sin_Usuario_Debe_Lanzar_AUTH_REQUIRED()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => Excel(usuario).Handle(ConsultaExcel(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("AUTH_REQUIRED");
    }

    [Fact]
    public async Task Excel_Sin_Cargo_Debe_Lanzar_AUTH_REQUIRED()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10);
        usuario.Setup(u => u.UserCargoId).Returns((long?)null);

        var act = () => Excel(usuario).Handle(ConsultaExcel(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("AUTH_REQUIRED");
    }

    [Fact]
    public async Task Excel_Debe_Devolver_El_Archivo_Generado()
    {
        var usuario = new Mock<ICurrentUserService>();
        var consultas = new Mock<ISolicitudQueryRepository>();
        var excel = new Mock<ISolicitudExcelReportService>();
        usuario.Setup(u => u.UserIdLocal).Returns(10);
        usuario.Setup(u => u.UserCargoId).Returns(3);
        usuario.Setup(u => u.UserAreaId).Returns(5);
        usuario.Setup(u => u.CodigoRolUsuario).Returns("SOL");
        consultas.Setup(c => c.ObtenerListadoSolicitud(It.IsAny<ReporteSolicitudExcelRequest>(), It.IsAny<CancellationToken>(), false))
            .ReturnsAsync([new SolicitudPaginadoDto(), new SolicitudPaginadoDto()]);
        excel.Setup(e => e.GenerarReporteSolicitudes(It.Is<List<SolicitudPaginadoDto>>(l => l.Count == 2)))
            .Returns(new byte[] { 1, 2 });

        var dto = await Excel(usuario, consultas, excel).Handle(ConsultaExcel(), CancellationToken.None);

        dto.Contenido.Should().Equal(1, 2);
        dto.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        dto.NombreArchivo.Should().StartWith("Reporte_Solicitudes_Cantidad_2_");
    }

    [Fact]
    public async Task Movimiento_Debe_Reservar_Id_Y_Confirmar()
    {
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var movimientos = new Mock<IMovimientoSolicitudRepository>();
        var uow = Unidad();
        correlativos.Setup(c => c.ObtenerSiguienteIdMovimientoSolicitudAsync(It.IsAny<CancellationToken>())).ReturnsAsync(50);
        movimientos.Setup(m => m.AddAsync(It.IsAny<MovimientoSolicitud>())).Returns(Task.CompletedTask);

        await Movimiento(correlativos, movimientos, uow);

        movimientos.Verify(m => m.AddAsync(It.Is<MovimientoSolicitud>(x => x.Id == 50 && x.IdSolicitud == 8)), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task Movimiento_Cuando_Falla_Debe_Revertir_Sin_Lanzar()
    {
        var correlativos = new Mock<IGeneradorCorrelativoIdRepository>();
        var movimientos = new Mock<IMovimientoSolicitudRepository>();
        var uow = Unidad();
        correlativos.Setup(c => c.ObtenerSiguienteIdMovimientoSolicitudAsync(It.IsAny<CancellationToken>())).ReturnsAsync(50);
        movimientos.Setup(m => m.AddAsync(It.IsAny<MovimientoSolicitud>())).ThrowsAsync(new InvalidOperationException("fallo"));

        var act = () => Movimiento(correlativos, movimientos, uow);

        await act.Should().NotThrowAsync();
        uow.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditarPlantilla_Sin_Plantilla_Debe_Lanzar_APP_SOL_005()
    {
        var usuario = new Mock<ICurrentUserService>();
        var plantillas = new Mock<IPlantillaRepository>();
        usuario.Setup(u => u.UserIdLocal).Returns(10);
        usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
        plantillas.Setup(p => p.ObtenerAgregadoPlantillaAsync(6, It.IsAny<CancellationToken>(), true))
            .ReturnsAsync((Plantilla?)null);

        var act = () => new EditarPlantillaCommandHandler(
                _mapper.Object, usuario.Object, plantillas.Object, new Mock<IParametroRepository>().Object)
            .Handle(new EditarPlantillaCommand(6, "Flujo", "Descripcion", []), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be("APP_SOL_005");
    }

    private ObtenerContextoUsuarioQueryHandler Contexto(
        Mock<ICurrentUserService> usuario,
        Mock<IAutenticacionExternaPort>? auth = null,
        Mock<IUsuarioRepository>? repo = null) =>
        new(_mapper.Object, usuario.Object, (auth ?? new Mock<IAutenticacionExternaPort>()).Object, (repo ?? new Mock<IUsuarioRepository>()).Object);

    private DetectarCodigoQueryHandler Detectar(
        Mock<IParametroRepository> parametros,
        Mock<ISolicitudQueryRepository>? solicitudes = null) =>
        new(_mapper.Object, (solicitudes ?? new Mock<ISolicitudQueryRepository>()).Object, parametros.Object);

    private PermisoSolicitudQueryHandler Permiso(
        Mock<ISolicitudRepository> solicitudes,
        Mock<IPlantillaRepository>? plantillas = null,
        Mock<ICurrentUserService>? usuario = null) =>
        new(
            _mapper.Object,
            solicitudes.Object,
            (usuario ?? UsuarioActual()).Object,
            (plantillas ?? new Mock<IPlantillaRepository>()).Object,
            new Mock<IParametroRepository>().Object,
            new Mock<COFIDE.WFF.Domain.DomainServices.IWorkflowDomainService>().Object);

    private DescargarReporteSolicitudExcelQueryHandler Excel(
        Mock<ICurrentUserService> usuario,
        Mock<ISolicitudQueryRepository>? consultas = null,
        Mock<ISolicitudExcelReportService>? excel = null) =>
        new(_mapper.Object, usuario.Object, (consultas ?? new Mock<ISolicitudQueryRepository>()).Object, (excel ?? new Mock<ISolicitudExcelReportService>()).Object);

    private Task Movimiento(
        Mock<IGeneradorCorrelativoIdRepository> correlativos,
        Mock<IMovimientoSolicitudRepository> movimientos,
        Mock<IUnitOfWork> uow) =>
        new RegistrarMovimientoSolicitudEventHandler(
                correlativos.Object, movimientos.Object, uow.Object, new Mock<ILogger<RegistrarMovimientoSolicitudEventHandler>>().Object)
            .Handle(new DomainEventNotification<SolicitudEstadoCambiadoDomainEvent>(
                new SolicitudEstadoCambiadoDomainEvent(8, 1, 10, "Anulado")), CancellationToken.None);

    private static Mock<IUnitOfWork> Unidad()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        uow.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        return uow;
    }

    private static Mock<ICurrentUserService> UsuarioActual(long? id = 10, string? codigo = "USR01")
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserIdLocal).Returns(id);
        usuario.Setup(u => u.CodigoUsuario).Returns(codigo);
        usuario.Setup(u => u.UserAreaId).Returns(5);
        usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
        return usuario;
    }

    private static UsuarioAutenticado Suc() => new(
        "WFF", "Firmas", "USR01", "Ana", "SOL", "Solicitante", "ana@cofide.com.pe", "jwt",
        [new MenuExternoDto(1, 0, "Inicio", null, null, null, null)],
        []);

    private static Solicitud Borrador() =>
        Solicitud.CrearBorrador(8, "GTI-SOL-00000008-2026", 1, 2, 10, 3, 10, 5, 2,
            "Contrato", null, false, false, null, 0);

    private static DescargarReporteSolicitudExcelQuery ConsultaExcel() =>
        new(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

    private static void Asignar(object destino, Type tipo, string propiedad, object valor)
    {
        var campo = tipo.GetField($"<{propiedad}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        campo!.SetValue(destino, valor);
    }
}
