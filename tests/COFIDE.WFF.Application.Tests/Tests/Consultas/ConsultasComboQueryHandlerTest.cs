using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Areas.Ports.Out;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;
using COFIDE.WFF.Application.Features.Areas.Queries.ObtenerDepartamentosCombo;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Jobs.ActualizarEstadoProgramado;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerListadoParametro;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitud;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacion;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitud;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoDocumentosCombo;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoSolicitudCombo;
using COFIDE.WFF.Application.Features.Usuarios.Commands.CerrarSesion;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;
using COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
using COFIDE.WFF.Domain.Entities;
using FluentAssertions;
using Moq;
using AppEx = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Tests.Consultas;

public class ConsultasComboQueryHandlerTest
{
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task Areas_Debe_Devolver_Lo_Que_Entrega_El_Repositorio()
    {
        var repo = new Mock<IAreaRepository>();
        var esperado = new List<AreasComboDto> { new() };
        repo.Setup(r => r.GetAreaCombo(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var resultado = await new ObtenerAreasComboQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerAreasComboQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task Departamentos_Debe_Reenviar_Los_Codigos_De_Gerencia()
    {
        var repo = new Mock<IAreaRepository>();
        var gerencias = new List<long> { 1, 2 };
        repo.Setup(r => r.GetDepartamentosCombo(gerencias, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepartamentosComboDto> { new() });

        var resultado = await new ObtenerDepartamentosComboQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerDepartamentosComboQuery(gerencias), CancellationToken.None);

        resultado.Should().ContainSingle();
        repo.Verify(r => r.GetDepartamentosCombo(gerencias, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Usuarios_Debe_Devolver_El_Combo_Del_Repositorio()
    {
        var repo = new Mock<IUsuarioRepository>();
        var esperado = new List<UsuariosComboDto> { new(), new() };
        repo.Setup(r => r.GetUsuarioCombo(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var resultado = await new ObtenerUsuariosComboQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerUsuariosComboQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task BuscarUsuarios_Debe_Reenviar_El_Termino()
    {
        var repo = new Mock<IUsuarioRepository>();
        var esperado = new List<UsuarioBuscadorDto> { new() };
        repo.Setup(r => r.BuscarAsync("ana", It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var resultado = await new BuscarUsuariosQueryHandler(_mapper.Object, repo.Object)
            .Handle(new BuscarUsuariosQuery("ana"), CancellationToken.None);

        resultado.Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task EstadosSolicitud_Debe_Devolver_El_Combo_Del_Repositorio()
    {
        var repo = new Mock<IEstadoSolicitudQueryRepository>();
        var esperado = new List<EstadoSolicitudComboDto> { new() };
        repo.Setup(r => r.GetEstadoSolicitudCombo(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var resultado = await new ObtenerEstadoSolicitudComboQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerEstadoSolicitudComboQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task EstadosDocumento_Debe_Devolver_El_Combo_Del_Repositorio()
    {
        var repo = new Mock<IEstadoDocumentoQueryRepository>();
        var esperado = new List<EstadoDocumentosComboDto> { new() };
        repo.Setup(r => r.GetEstadoDocumentoCombo(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var resultado = await new ObtenerEstadoDocumentosComboQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerEstadoDocumentosComboQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task TipoSolicitudPorArea_Debe_Reenviar_Las_Areas()
    {
        var repo = new Mock<IMaestroRepository>();
        var areas = new List<long> { 5 };
        repo.Setup(r => r.GetTipoSolicitudCombo(areas, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoSolicitudComboDto> { new() });

        var resultado = await new ObtenerTipoSolicitudPorAreaQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerTipoSolicitudPorAreaQuery(areas), CancellationToken.None);

        resultado.Should().ContainSingle();
    }

    [Fact]
    public async Task SubTipoSolicitudPorArea_Debe_Reenviar_Area_Y_Tipo()
    {
        var repo = new Mock<IMaestroRepository>();
        var areas = new List<long> { 5 };
        var tipos = new List<long> { 2 };
        repo.Setup(r => r.GetSubTipoSolicitudCombo(areas, tipos, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubTipoSolicitudComboDto> { new() });

        var resultado = await new ObtenerSubTipoSolicitudPorAreaQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerSubTipoSolicitudPorAreaQuery(areas, tipos), CancellationToken.None);

        resultado.Should().ContainSingle();
    }

    [Fact]
    public async Task TipoOperacionPorArea_Debe_Reenviar_Las_Areas()
    {
        var repo = new Mock<IMaestroRepository>();
        var areas = new List<long> { 3 };
        repo.Setup(r => r.GetTipoOperacionCombo(areas, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoOperacionComboDto> { new() });

        var resultado = await new ObtenerTipoOperacionPorAreaQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerTipoOperacionPorAreaQuery(areas), CancellationToken.None);

        resultado.Should().ContainSingle();
    }

    [Fact]
    public async Task TipoSolicitud_Debe_Usar_El_Area_Del_Usuario_Y_Mapear()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserAreaId).Returns(5);
        var repo = new Mock<IMaestroRepository>();
        var entidades = new List<TipoSolicitud>();
        var mapeado = new List<TipoSolicitudDto> { new() };

        repo.Setup(r => r.GetTipoSolicitudByIdAreaAsync(5, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(entidades);
        _mapper.Setup(m => m.Map<List<TipoSolicitudDto>>(entidades)).Returns(mapeado);

        var resultado = await new ObtenerTipoSolicitudQueryHandler(_mapper.Object, usuario.Object, repo.Object)
            .Handle(new ObtenerTipoSolicitudQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(mapeado);
    }

    [Fact]
    public async Task SubTipoSolicitud_Sin_Area_Debe_Lanzar_APP_SOL_018()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserAreaId).Returns((long?)null);

        var act = () => new ObtenerSubTipoSolicitudQueryHandler(
            _mapper.Object, usuario.Object, new Mock<IMaestroRepository>().Object)
            .Handle(new ObtenerSubTipoSolicitudQuery(2), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UsuarioAreaAsignado.Code);
    }

    [Fact]
    public async Task SubTipoSolicitud_Debe_Filtrar_Por_Area_Del_Usuario_Y_Tipo()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserAreaId).Returns(5);
        var repo = new Mock<IMaestroRepository>();
        var entidades = new List<SubTipoSolicitud>();
        var mapeado = new List<SubTipoSolicitudDto> { new() };

        repo.Setup(r => r.GetByIdTipoSolicitudAndIdAreaAsync(5, 2, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(entidades);
        _mapper.Setup(m => m.Map<List<SubTipoSolicitudDto>>(entidades)).Returns(mapeado);

        var resultado = await new ObtenerSubTipoSolicitudQueryHandler(_mapper.Object, usuario.Object, repo.Object)
            .Handle(new ObtenerSubTipoSolicitudQuery(2), CancellationToken.None);

        resultado.Should().BeSameAs(mapeado);
    }

    [Fact]
    public async Task TipoOperacion_Sin_Area_Debe_Lanzar_APP_SOL_018()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserAreaId).Returns((long?)null);

        var act = () => new ObtenerTipoOperacionQueryHandler(
            _mapper.Object, usuario.Object, new Mock<IMaestroRepository>().Object)
            .Handle(new ObtenerTipoOperacionQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppEx.ApplicationException>();
        ex.Which.Error.Code.Should().Be(ApplicationErrors.UsuarioAreaAsignado.Code);
    }

    [Fact]
    public async Task TipoOperacion_Debe_Filtrar_Por_Area_Del_Usuario_Y_Mapear()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.UserAreaId).Returns(5);
        var repo = new Mock<IMaestroRepository>();
        var entidades = new List<TipoOperacion>();
        var mapeado = new List<TipoOperacionDto> { new() };

        repo.Setup(r => r.GetByIdAreaAsync(5, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(entidades);
        _mapper.Setup(m => m.Map<List<TipoOperacionDto>>(entidades)).Returns(mapeado);

        var resultado = await new ObtenerTipoOperacionQueryHandler(_mapper.Object, usuario.Object, repo.Object)
            .Handle(new ObtenerTipoOperacionQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(mapeado);
    }

    [Fact]
    public async Task ListadoParametro_Debe_Mapear_Lo_Que_Devuelve_El_Repositorio()
    {
        var repo = new Mock<IParametroRepository>();
        var entidades = new List<ParametroDetalle>();
        var mapeado = new List<ListadoParametroDto> { new() };

        repo.Setup(r => r.ObtenerParametrosAsync(It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(entidades);
        _mapper.Setup(m => m.Map<List<ListadoParametroDto>>(entidades)).Returns(mapeado);

        var resultado = await new ObtenerListadoParametroQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerListadoParametroQuery(), CancellationToken.None);

        resultado.Should().BeSameAs(mapeado);
    }

    [Fact]
    public async Task PlantillaPorId_Debe_Devolver_El_Detalle_Del_Repositorio()
    {
        var repo = new Mock<IPlantillaRepository>();
        var esperado = new PlantillaDto();
        repo.Setup(r => r.ObtenerDetallePlantillaPorId(7)).ReturnsAsync(esperado);

        var resultado = await new ObtenerPlantillaPorIdQueryHandler(_mapper.Object, repo.Object)
            .Handle(new ObtenerPlantillaPorIdQuery(7), CancellationToken.None);

        resultado.Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task CerrarSesion_Debe_Delegar_En_El_Puerto_Externo()
    {
        var auth = new Mock<IAutenticacionExternaPort>();
        auth.Setup(a => a.CerrarSesionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var resultado = await new CerrarSesionCommandHandler(_mapper.Object, auth.Object)
            .Handle(new CerrarSesionCommand(), CancellationToken.None);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ActualizarEstadosProgramado_Debe_Consultar_El_Parametro_Y_Retornar_Uno()
    {
        var repo = new Mock<IParametroRepository>();
        repo.Setup(r => r.ObtenerParametroDetallePorIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParametroDetalle?)null);

        var resultado = await new ActualizarEstadosProgramadoCommandHandler(repo.Object)
            .Handle(new ActualizarEstadosProgramadoCommand(4), CancellationToken.None);

        resultado.Should().Be(1);
        repo.Verify(r => r.ObtenerParametroDetallePorIdAsync(4, It.IsAny<CancellationToken>()), Times.Once);
    }
}
