using System.Reflection;
using System.Security.Claims;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using COFIDE.WFF.Infrastructure.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public sealed class SolicitudVisibilidadPaginadoTests
{
    private const long IdUsuario = 20;
    private const long AreaUsuario = 90;

    [Fact]
    public async Task Paginado_Debe_Aplicar_Acceso_Especial_Al_Universo_Visible_Y_Calcular_Resumen()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var seed = CrearContexto(databaseName))
        {
            await SembrarSolicitudesAsync(seed);
        }

        await using var contexto = CrearContexto(databaseName);
        var repository = CrearRepositorio(contexto);

        var resultado = await repository.ObtenerPaginado(
            new PagedQueryParams { PageNumber = 1, PageSize = 10 },
            IdUsuario,
            AreaUsuario,
            "COORD",
            AreaUsuario,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true,
            [5],
            CancellationToken.None);

        resultado.Items.Select(s => s.IdSolicitud).Should().BeEquivalentTo(new long[] { 1, 2, 3 });
        resultado.TotalCount.Should().Be(3);
        resultado.ExtraData!.Total.Should().Be(3);
        resultado.ExtraData.Urgente.Should().Be(2);
        resultado.ExtraData.MisSolicitudes.Should().Be(2);
        resultado.ExtraData.MiDepartamento.Should().Be(2);
    }

    [Fact]
    public async Task MisSolicitudes_Desde_Handler_Y_Repositorios_Reales_Debe_Cumplir_CA01_Y_CA10()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var seed = CrearContexto(databaseName))
        {
            await SembrarSolicitudesAsync(seed);
        }

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("UserIdLocal", IdUsuario.ToString()),
            new Claim("UserCargoId", "4"),
            new Claim("UserAreaId", AreaUsuario.ToString()),
            new Claim("UserCodigoRol", "COORD"),
            new Claim("UserRol", RolUsuario.Coordinador.ToString())
        ], "Test"));
        var currentUser = new CurrentUserService(new HttpContextAccessor { HttpContext = httpContext });

        await using var contexto = CrearContexto(databaseName, currentUser);
        var handler = new ObtenerSolicitudPaginadoQueryHandler(
            currentUser,
            CrearRepositorio(contexto),
            new AccesoEspecialRepository(contexto));
        var query = new ObtenerSolicitudPaginadoQuery(
            null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, true)
        {
            PageNumber = 1,
            PageSize = 10
        };

        var resultado = await handler.Handle(query, CancellationToken.None);

        resultado.ListadoSolicitudItem.Select(s => s.IdSolicitud)
            .Should().BeEquivalentTo(new long[] { 1, 2, 3 });
        resultado.Total.Should().Be(3);
        resultado.Urgente.Should().Be(2);
        resultado.MisSolicitudes.Should().Be(2);
        resultado.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Paginado_PaginaSoloUnaSolicitud_Pero_Conserva_ConteosDelUniversoVisible()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var seed = CrearContexto(databaseName))
        {
            await SembrarSolicitudesAsync(seed);
        }

        await using var contexto = CrearContexto(databaseName);
        var resultado = await ObtenerPaginado(
            CrearRepositorio(contexto), pageNumber: 2, pageSize: 1);

        resultado.Items.Should().ContainSingle();
        resultado.Items.Single().IdSolicitud.Should().BeOneOf(1L, 2L, 3L);
        resultado.TotalCount.Should().Be(3);
        resultado.ExtraData!.Total.Should().Be(3);
        resultado.ExtraData.Urgente.Should().Be(2);
        resultado.ExtraData.MisSolicitudes.Should().Be(2);
    }

    [Fact]
    public async Task Paginado_Filtros_No_Amplian_Acceso_Y_La_Gerencia_Incluye_Sus_Departamentos()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var seed = CrearContexto(databaseName))
        {
            await SembrarSolicitudesAsync(seed);
        }

        await using var contexto = CrearContexto(databaseName);
        var repository = CrearRepositorio(contexto);

        var gerencia = await ObtenerPaginado(repository, gerencias: [5]);
        gerencia.Items.Select(s => s.IdSolicitud).Should().BeEquivalentTo(new long[] { 2, 3 });
        gerencia.TotalCount.Should().Be(2);

        var departamentoAutorizado = await ObtenerPaginado(repository, departamentos: [50]);
        departamentoAutorizado.Items.Select(s => s.IdSolicitud).Should().Equal(2);

        var accesoSoloDepartamento = await ObtenerPaginado(repository, areasAccesoEspecial: [50]);
        accesoSoloDepartamento.Items.Select(s => s.IdSolicitud).Should().BeEquivalentTo(new long[] { 1, 2 });
        accesoSoloDepartamento.Items.Select(s => s.IdSolicitud).Should().NotContain(3);

        var sinAccesoEspecial = await ObtenerPaginado(repository, areasAccesoEspecial: []);
        sinAccesoEspecial.Items.Select(s => s.IdSolicitud).Should().Equal(1);

        var departamentoNoAutorizado = await ObtenerPaginado(repository, departamentos: [70]);
        departamentoNoAutorizado.Items.Should().BeEmpty();
        departamentoNoAutorizado.TotalCount.Should().Be(0);
    }

    private static async Task<PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>> ObtenerPaginado(
        ISolicitudQueryRepository repository,
        List<long>? gerencias = null,
        List<long>? departamentos = null,
        int pageNumber = 1,
        int pageSize = 10,
        List<long>? areasAccesoEspecial = null) =>
        await repository.ObtenerPaginado(
            new PagedQueryParams { PageNumber = pageNumber, PageSize = pageSize },
            IdUsuario,
            AreaUsuario,
            "COORD",
            AreaUsuario,
            null,
            null,
            gerencias,
            departamentos,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            true,
            areasAccesoEspecial ?? [5],
            CancellationToken.None);

    private static async Task SembrarSolicitudesAsync(WorkflowFirmaDbContext contexto)
    {
        var gerenciaAcceso = CrearArea(5, null, "Gerencia Especial");
        var departamento50 = CrearArea(50, 5, "Departamento 50");
        var departamento51 = CrearArea(51, 5, "Departamento 51");
        var departamento70 = CrearArea(70, 7, "Departamento 70");
        var departamento90 = CrearArea(90, 9, "Departamento Usuario");
        contexto.Areas.AddRange(gerenciaAcceso, departamento50, departamento51, departamento70, departamento90);

        var tipo = CrearEntidad<TipoSolicitud>(1);
        Asignar(tipo, nameof(TipoSolicitud.NombreTipoSolicitud), "Tipo");
        Asignar(tipo, nameof(TipoSolicitud.DescripcionCorta), "T");
        Asignar(tipo, nameof(TipoSolicitud.IdArea), 5L);
        Asignar(tipo, nameof(TipoSolicitud.Area), gerenciaAcceso);
        contexto.TipoSolicitudes.Add(tipo);

        var subtipo = CrearEntidad<SubTipoSolicitud>(2);
        Asignar(subtipo, nameof(SubTipoSolicitud.NombreSubTipoSolicitud), "Subtipo");
        Asignar(subtipo, nameof(SubTipoSolicitud.IdTipoSolicitud), 1L);
        Asignar(subtipo, nameof(SubTipoSolicitud.IdArea), 5L);
        Asignar(subtipo, nameof(SubTipoSolicitud.TipoSolicitud), tipo);
        Asignar(subtipo, nameof(SubTipoSolicitud.Area), gerenciaAcceso);
        contexto.SubTipoSolicitud.Add(subtipo);

        var operacion = CrearEntidad<TipoOperacion>(3);
        Asignar(operacion, nameof(TipoOperacion.NombreTipoOperacion), "Operación");
        Asignar(operacion, nameof(TipoOperacion.IdArea), 5L);
        Asignar(operacion, nameof(TipoOperacion.Area), gerenciaAcceso);
        contexto.TipoOperacion.Add(operacion);

        var solicitantePropia = CrearUsuario(IdUsuario, 90);
        var solicitante51 = CrearUsuario(21, 51);
        var solicitanteNoAutorizado = CrearUsuario(22, 70);
        contexto.Usuarios.AddRange(solicitantePropia, solicitante51, solicitanteNoAutorizado);

        var accesoEspecial = AccesoEspecial.Crear(900, IdUsuario, 5);
        Asignar(accesoEspecial, nameof(AccesoEspecial.Usuario), solicitantePropia);
        Asignar(accesoEspecial, nameof(AccesoEspecial.Area), gerenciaAcceso);
        contexto.AccesosEspeciales.Add(accesoEspecial);

        var plantilla = Plantilla.Crear(1, "Plantilla", "Prueba", false, IdUsuario, "GTI", true);
        contexto.Plantillas.Add(plantilla);

        var solicitudes = new[]
        {
            CrearSolicitud(1, 90, 9, IdUsuario, urgente: true, solicitantePropia, plantilla, tipo, subtipo, operacion),
            CrearSolicitud(2, 50, 5, IdUsuario, urgente: false, solicitantePropia, plantilla, tipo, subtipo, operacion),
            CrearSolicitud(3, 51, 5, 21, urgente: true, solicitante51, plantilla, tipo, subtipo, operacion),
            CrearSolicitud(4, 70, 7, 22, urgente: true, solicitanteNoAutorizado, plantilla, tipo, subtipo, operacion)
        };
        contexto.Solicitudes.AddRange(solicitudes);
        await contexto.SaveChangesAsync();
    }

    private static Solicitud CrearSolicitud(
        long id,
        long departamento,
        long gerencia,
        long idSolicitante,
        bool urgente,
        Usuario usuario,
        Plantilla plantilla,
        TipoSolicitud tipo,
        SubTipoSolicitud subtipo,
        TipoOperacion operacion)
    {
        var solicitud = Solicitud.CrearBorrador(
            id, $"SOL-{id}", 1, 2, idSolicitante, 3, idSolicitante,
            departamento, gerencia, $"Solicitud {id}", null, urgente, false,
            urgente ? DateTime.Now.AddDays(1) : null, 0);
        Asignar(solicitud, nameof(Solicitud.TipoSolicitud), tipo);
        Asignar(solicitud, nameof(Solicitud.SubTipoSolicitud), subtipo);
        Asignar(solicitud, nameof(Solicitud.TipoOperacion), operacion);
        Asignar(solicitud, nameof(Solicitud.UsuarioSolicitante), usuario);
        solicitud.AsignarPlantilla(SolicitudPlantilla.Crear(id, solicitud, plantilla));
        return solicitud;
    }

    private static Usuario CrearUsuario(long id, long areaId)
    {
        var usuario = new Usuario(id, RolUsuario.SolicitanteAprobador);
        Asignar(usuario, nameof(Usuario.Nombres), "Nombre");
        Asignar(usuario, nameof(Usuario.ApellidoPaterno), "Prueba");
        Asignar(usuario, nameof(Usuario.ApellidoMaterno), "Usuario");
        Asignar(usuario, nameof(Usuario.NumeroDocumento), $"DOC{id}");
        Asignar(usuario, nameof(Usuario.Email), $"usuario{id}@example.test");
        Asignar(usuario, nameof(Usuario.CuentaLogueo), $"USR{id}");
        Asignar(usuario, "IdArea", (long?)areaId);
        return usuario;
    }

    private static Area CrearArea(long id, long? idPadre, string nombre)
    {
        var area = CrearEntidad<Area>(id);
        Asignar(area, nameof(Area.CodDependencia), $"A{id}");
        Asignar(area, nameof(Area.NombreArea), nombre);
        Asignar(area, nameof(Area.DescripcionCorta), nombre);
        Asignar(area, "IdAreaPadre", idPadre);
        return area;
    }

    private static T CrearEntidad<T>(long id) where T : class
    {
        var entidad = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
        Asignar(entidad, "Id", id);
        return entidad;
    }

    private static void Asignar(object entidad, string propiedad, object? valor)
    {
        var info = entidad.GetType().GetProperty(propiedad, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (info?.SetMethod is not null)
        {
            info.SetValue(entidad, valor);
            return;
        }

        var campo = entidad.GetType().GetField($"<{propiedad}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"No se encontró la propiedad persistida {entidad.GetType().Name}.{propiedad}.");
        campo.SetValue(entidad, valor);
    }

    private static WorkflowFirmaDbContext CrearContexto(
        string nombreBaseDatos,
        COFIDE.WFF.Application.Common.Ports.Out.ICurrentUserService? currentUser = null)
    {
        var opciones = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(nombreBaseDatos)
            .Options;
        return new WorkflowFirmaDbContext(opciones, currentUser!);
    }

    private static SolicitudQueryRepository CrearRepositorio(WorkflowFirmaDbContext contexto) =>
        new(contexto, null!, TimeProvider.System);
}
