using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Firmas.Commands.ValidarEstadoDetalleFirma;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPanelControl;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Common.Pageds;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Application.Tests.Support;

namespace COFIDE.WFF.Application.Tests.Tests.Solicitudes;

public class SolicitudConsultaHandlerTests
{
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUserService> _usuario = new();

    [Fact]
    public async Task ObtenerSolicitud_Sin_Usuario_Debe_Lanzar_AUTH_REQUIRED()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => new ObtenerSolicitudQueryHandler(
            _mapper.Object,
            _usuario.Object,
            new Mock<IDocumentoFirmadoValidator>().Object,
            ArchivosDePrueba.Almacenamiento().Object,
            new Mock<ISolicitudQueryRepository>().Object)
            .Handle(new ObtenerSolicitudQuery(1, false), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UserNotAuthenticated.Code);
    }

    [Fact]
    public async Task ObtenerSolicitud_En_Reuso_No_Debe_Leer_Archivos()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        var archivos = ArchivosDePrueba.Almacenamiento();
        var consulta = new Mock<ISolicitudQueryRepository>();
        var dto = new SolicitudDto
        {
            Documentos = [new ArchivoDto { RutaArchivo = "/doc/contrato.pdf" }]
        };
        consulta.Setup(r => r.ObtenerSolicitudPorId(1, 10, true, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(dto);

        var resultado = await new ObtenerSolicitudQueryHandler(
            _mapper.Object, _usuario.Object, new Mock<IDocumentoFirmadoValidator>().Object,
            archivos.Object, consulta.Object)
            .Handle(new ObtenerSolicitudQuery(1, true), CancellationToken.None);

        resultado.Should().BeSameAs(dto);
        resultado!.Documentos.Single().Firmado.Should().BeFalse();
        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerSolicitud_Debe_Marcar_Los_Documentos_Que_Ya_Tienen_Firma()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        var archivos = ArchivosDePrueba.Almacenamiento();
        var validador = new Mock<IDocumentoFirmadoValidator>();
        var consulta = new Mock<ISolicitudQueryRepository>();
        var dto = new SolicitudDto
        {
            Documentos =
            [
                new ArchivoDto { RutaArchivo = "/doc/firmado.pdf" },
                new ArchivoDto { RutaArchivo = "/doc/limpio.pdf" }
            ]
        };

        consulta.Setup(r => r.ObtenerSolicitudPorId(1, 10, false, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(dto);
        archivos.Setup(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream([1]));
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>()))
            .ReturnsAsync((Stream _) => true);

        var resultado = await new ObtenerSolicitudQueryHandler(
            _mapper.Object, _usuario.Object, validador.Object, archivos.Object, consulta.Object)
            .Handle(new ObtenerSolicitudQuery(1, false), CancellationToken.None);

        resultado!.Documentos.Should().OnlyContain(d => d.Firmado);
        archivos.Verify(a => a.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task BandejaEntrada_Sin_Usuario_Debe_Lanzar_AUTH_REQUIRED()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => new ObtenerBandejaEntradaQueryHandler(
            _usuario.Object,
            new Mock<ISolicitudQueryRepository>().Object)
            .Handle(QueryBandeja(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UserNotAuthenticated.Code);
    }

    [Fact]
    public async Task BandejaEntrada_Debe_Proyectar_Resumen_E_Items()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        var repo = new Mock<ISolicitudQueryRepository>();
        var items = new List<BandejaEntradaDto> { new() { CodigoSolicitud = "GTI-SOL-00000001-2026" } };
        var pagina = new PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>(items, 12, 1, 10)
        {
            ExtraData = new BandejaEntradaResumenDto { Total = 12, Urgente = 2 }
        };
        repo.Setup(r => r.ObtenerBandejaEntrada(
                It.IsAny<PagedQueryParams>(), 10,
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagina);

        var resultado = await new ObtenerBandejaEntradaQueryHandler(
            _usuario.Object, repo.Object)
            .Handle(QueryBandeja(), CancellationToken.None);

        resultado.Total.Should().Be(12);
        resultado.Urgente.Should().Be(2);
        resultado.TotalCount.Should().Be(12);
        resultado.ListadoSolicitudes.Should().ContainSingle();
        resultado.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task BandejaEntrada_Sin_Resumen_Debe_Devolver_Ceros()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        var repo = new Mock<ISolicitudQueryRepository>();
        repo.Setup(r => r.ObtenerBandejaEntrada(
                It.IsAny<PagedQueryParams>(), It.IsAny<long?>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<List<long>>(), It.IsAny<List<long>>(), It.IsAny<List<long>>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<bool?>(), It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>([], 0, 1, 10));

        var resultado = await new ObtenerBandejaEntradaQueryHandler(
            _usuario.Object, repo.Object)
            .Handle(QueryBandeja(), CancellationToken.None);

        resultado.Total.Should().Be(0);
        resultado.Urgente.Should().Be(0);
        resultado.ListadoSolicitudes.Should().BeEmpty();
    }

    [Fact]
    public async Task PanelControl_Sin_Usuario_Debe_Lanzar_AUTH_REQUIRED()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => new ObtenerSolicitudPanelControlQueryHandler(
            _mapper.Object, _usuario.Object, new Mock<ISolicitudQueryRepository>().Object)
            .Handle(new ObtenerSolicitudPanelControlQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UserNotAuthenticated.Code);
    }

    [Fact]
    public async Task PanelControl_Debe_Consultar_Con_El_Usuario_Actual()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        var consulta = new Mock<ISolicitudQueryRepository>();
        var esperado = new SolicitudPanelControlDto();
        consulta.Setup(r => r.ObtenerSolicitudPanelControl(10, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(esperado);

        var resultado = await new ObtenerSolicitudPanelControlQueryHandler(
            _mapper.Object, _usuario.Object, consulta.Object)
            .Handle(new ObtenerSolicitudPanelControlQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task ValidarDocumentoFirmado_Debe_Evaluar_Cada_Archivo()
    {
        var archivo = ArchivosDePrueba.Crear("contrato.pdf", [1, 2]);
        var validador = new Mock<IDocumentoFirmadoValidator>();
        validador.Setup(v => v.TieneFirmaAsync(It.IsAny<Stream>())).ReturnsAsync(true);

        var resultado = await new ValidarDocumentoFirmadoQueryHandler(_mapper.Object, validador.Object)
            .Handle(new ValidarDocumentoFirmadoQuery([archivo]), CancellationToken.None);

        resultado.Should().ContainSingle();
        resultado.Single().NombreDocumento.Should().Be("contrato.pdf");
        resultado.Single().TieneFirmaDigital.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarEstado_Sin_Detalle_En_Firma_No_Debe_Bloquear()
    {
        var repo = new Mock<IDetalleFirmaRepository>();
        repo.Setup(r => r.ValidarDocumentoFirmadoAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var resultado = await new ValidarEstadoDetalleFirmaCommandHandler(
            _mapper.Object, _usuario.Object, repo.Object)
            .Handle(new ValidarEstadoDetalleFirmaCommand(1, [10]), CancellationToken.None);

        resultado!.DocumentoBloqueado.Should().BeFalse();
    }

    [Fact]
    public async Task ValidarEstado_Del_Mismo_Firmante_No_Debe_Bloquear()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(100);
        var detalle = DetalleFirma.Crear(1, 10, 20, null, null, null, null, 100);
        var repo = new Mock<IDetalleFirmaRepository>();
        repo.Setup(r => r.ValidarDocumentoFirmadoAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);

        var resultado = await new ValidarEstadoDetalleFirmaCommandHandler(
            _mapper.Object, _usuario.Object, repo.Object)
            .Handle(new ValidarEstadoDetalleFirmaCommand(1, [20]), CancellationToken.None);

        resultado!.DocumentoBloqueado.Should().BeFalse();
    }

    [Fact]
    public async Task ValidarEstado_De_Otro_Firmante_Debe_Bloquear_El_Documento()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(99);
        var detalle = DetalleFirma.Crear(1, 10, 20, null, null, null, null, 100);
        var repo = new Mock<IDetalleFirmaRepository>();
        repo.Setup(r => r.ValidarDocumentoFirmadoAsync(It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([detalle]);

        var resultado = await new ValidarEstadoDetalleFirmaCommandHandler(
            _mapper.Object, _usuario.Object, repo.Object)
            .Handle(new ValidarEstadoDetalleFirmaCommand(1, [20]), CancellationToken.None);

        resultado!.DocumentoBloqueado.Should().BeTrue();
        resultado.Mensaje.Should().Contain("Documento bloqueado");
    }

    [Fact]
    public async Task ValidarExistencia_Sin_Coincidencia_Debe_Responder_Que_No_Existe()
    {
        ConfigurarUsuarioPlantilla();
        var repo = new Mock<IPlantillaRepository>();
        repo.Setup(r => r.ValidarExistenciaNombrePlantilla("Flujo", false, 10))
            .ReturnsAsync((Plantilla?)null);

        var resultado = await new ValidarExistenciaQueryHandler(_mapper.Object, _usuario.Object, repo.Object)
            .Handle(new ValidarExistenciaQuery("Flujo", false), CancellationToken.None);

        resultado!.Existe.Should().BeFalse();
        resultado.IdPlantilla.Should().Be(0);
    }

    [Fact]
    public async Task ValidarExistencia_Con_Permiso_Debe_Ofrecer_Reemplazar()
    {
        ConfigurarUsuarioPlantilla();
        var plantilla = Plantilla.Crear(4, "Flujo", "Desc", false, 10, "GTI", true);
        var repo = new Mock<IPlantillaRepository>();
        repo.Setup(r => r.ValidarExistenciaNombrePlantilla("Flujo", false, 10)).ReturnsAsync(plantilla);

        var resultado = await new ValidarExistenciaQueryHandler(_mapper.Object, _usuario.Object, repo.Object)
            .Handle(new ValidarExistenciaQuery("Flujo", false), CancellationToken.None);

        resultado!.Existe.Should().BeTrue();
        resultado.IdPlantilla.Should().Be(4);
        resultado.BotonSecundario.Should().Be("Reemplazar");
    }

    [Fact]
    public async Task RegistrarPlantilla_Debe_Persistir_La_Plantilla_Creada()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        _usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        var plantilla = Plantilla.Crear(6, "Flujo", "Desc", false, 10, "GTI", true);
        var servicio = new Mock<IPlantillaCreacionService>();
        var repo = new Mock<IPlantillaRepository>();
        var input = new CrearPlantillaInput { NombrePlantilla = "Flujo" };

        _mapper.Setup(m => m.Map<CrearPlantillaInput>(It.IsAny<RegistrarPlantillaCommand>())).Returns(input);
        servicio.Setup(s => s.CrearPlantilla(input, 10, "GTI", It.IsAny<CancellationToken>())).ReturnsAsync(plantilla);
        repo.Setup(r => r.AddAsync(plantilla)).Returns(Task.CompletedTask);

        var id = await new RegistrarPlantillaCommandHandler(
            _mapper.Object, _usuario.Object, repo.Object, servicio.Object)
            .Handle(new RegistrarPlantillaCommand("Flujo", "Desc", false, true, null), CancellationToken.None);

        id.Should().Be(6);
        repo.Verify(r => r.AddAsync(plantilla), Times.Once);
    }

    [Fact]
    public async Task PlantillaPaginado_Debe_Usar_Rol_Gerencia_Y_Propietario_Del_Usuario()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        _usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.Coordinador);
        _usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        var repo = new Mock<IPlantillaRepository>();
        PlantillaPaginadoRequest? recibido = null;
        repo.Setup(r => r.ObtenerPaginado(It.IsAny<PlantillaPaginadoRequest>()))
            .Callback<PlantillaPaginadoRequest>(r => recibido = r)
            .ReturnsAsync(new PagedResult<PlantillaPaginadoDto>([new PlantillaPaginadoDto()], 1, 1, 10));

        var query = new ObtenerPlantillaPaginadoQuery("flujo", true, [1], ["GTI"])
        {
            PageNumber = 2,
            PageSize = 10,
            SearchTerm = "flujo"
        };

        var resultado = await new ObtenerPlantillaPaginadoQueryHandler(repo.Object, _usuario.Object)
            .Handle(query, CancellationToken.None);

        recibido!.RolUsuario.Should().Be(RolUsuario.Coordinador);
        recibido.AreaParaCoordinador.Should().Be("GTI");
        recibido.IdUsuarioPropietario.Should().Be(10);
        recibido.PaginationParams.PageNumber.Should().Be(2);
        resultado.TotalCount.Should().Be(1);
        resultado.Items.Should().ContainSingle();
    }

    private void ConfigurarUsuarioPlantilla()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        _usuario.Setup(u => u.UserGerenciaNombre).Returns("GTI");
        _usuario.Setup(u => u.RolUsuario).Returns(RolUsuario.SolicitanteAprobador);
    }

    private static ObtenerBandejaEntradaQuery QueryBandeja() =>
        new(null, null, null, null, null, null, null, null, null, null, null, null, true)
        {
            PageNumber = 1,
            PageSize = 10
        };
}
