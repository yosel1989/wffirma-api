using System.Reflection;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

public class ReglaConsultaSolicitudTest
{
    [Theory]
    [InlineData("COORD", 50L, null, true)]
    [InlineData("COORD", 99L, null, false)]
    [InlineData("COORD", 99L, 50L, true)]
    [InlineData("COORD", 99L, 5L, true)]
    [InlineData("SOLAPR", 99L, null, false)]
    [InlineData("SOLAPR", 99L, 50L, true)]
    [InlineData("SOLAPR", 99L, 5L, true)]
    [InlineData("SOLAPR", 99L, 60L, false)]
    [InlineData("ADMIN", 99L, null, true)]
    public void Consulta_Y_Predicado_Deben_Aplicar_La_Misma_Regla(
        string rol, long areaUsuario, long? areaEspecial, bool autorizado)
    {
        var solicitud = CrearSolicitud(1, 50, 5);
        long[] areas = areaEspecial.HasValue ? [areaEspecial.Value] : [];
        var predicado = ReglaConsultaSolicitud.CrearPredicado(20, areaUsuario, rol, areas);

        new[] { solicitud }.AsQueryable().Where(predicado).Select(s => s.Id)
            .Should().Equal(autorizado ? new long[] { 1 } : []);

        Action validar = () => ReglaConsultaSolicitud.Validar(solicitud, 20, areaUsuario, rol, areas);
        if (autorizado)
            validar.Should().NotThrow();
        else
            validar.Should().Throw<DomainException>()
                .Which.Error.Should().Be(SolicitudErrors.PermisoVisualizarSolicitud);
    }

    [Theory]
    [InlineData("COORD")]
    [InlineData("SOLAPR")]
    public void Gerencia_Incluye_Departamentos_Pero_Departamento_No_Incluye_Hermanos(string rol)
    {
        var solicitudes = new[]
        {
            CrearSolicitud(1, 50, 5),
            CrearSolicitud(2, 60, 5),
            CrearSolicitud(3, 70, 7)
        }.AsQueryable();

        solicitudes.Where(ReglaConsultaSolicitud.CrearPredicado(20, 99, rol, [5]))
            .Select(s => s.Id).Should().Equal(1, 2);
        solicitudes.Where(ReglaConsultaSolicitud.CrearPredicado(20, 99, rol, [50]))
            .Select(s => s.Id).Should().Equal(1);
        solicitudes.Where(ReglaConsultaSolicitud.CrearPredicado(20, 99, rol, []))
            .Should().BeEmpty();
    }

    [Fact]
    public void Acceso_Especial_No_Debe_Reemplazar_Visibilidad_Propia()
    {
        var solicitudes = new[] { CrearSolicitud(1, 50, 5), CrearSolicitud(2, 70, 7) }.AsQueryable();

        solicitudes.Where(ReglaConsultaSolicitud.CrearPredicado(10, 99, "SOLAPR", null))
            .Select(s => s.Id).Should().Equal(1, 2);
        solicitudes.Where(ReglaConsultaSolicitud.CrearPredicado(10, 99, "SOLAPR", [5]))
            .Select(s => s.Id).Should().Equal(1, 2);
    }

    [Fact]
    public void Filtro_De_Area_No_Debe_Recuperar_Solicitudes_Fuera_De_La_Visibilidad()
    {
        var solicitudes = new[] { CrearSolicitud(1, 50, 5), CrearSolicitud(2, 70, 7) }.AsQueryable();
        var visibles = solicitudes.Where(ReglaConsultaSolicitud.CrearPredicado(20, 99, "SOLAPR", [5]));

        solicitudes.Where(s => s.UsuarioSolicitante.IdArea == 70)
            .Select(s => s.Id).Should().Equal(2);
        visibles.Select(s => s.Id).Should().Equal(1);
        visibles.Where(s => s.UsuarioSolicitante.IdArea == 70).Should().BeEmpty();
        visibles.Where(s => s.UsuarioSolicitante.IdArea == 50)
            .Select(s => s.Id).Should().Equal(1);
    }

    [Fact]
    public void Solicitud_Inexistente_Debe_Rechazarse_Incluso_Para_Administrador()
    {
        Action validar = () => ReglaConsultaSolicitud.Validar(null, 20, 99, "ADMIN", []);

        validar.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DOM_SOL_034");
    }

    [Theory]
    [InlineData("COORD")]
    [InlineData("SOLAPR")]
    public void Area_O_Padre_Nulos_No_Deben_Provocar_Error_Ni_Otorgar_Acceso(string rol)
    {
        var sinPadre = CrearSolicitud(1, 50, null);
        var sinArea = CrearSolicitud(2, null, null);
        var predicado = ReglaConsultaSolicitud.CrearPredicado(20, 99, rol, [5]);

        new[] { sinPadre, sinArea }.AsQueryable().Where(predicado).Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("DESCONOCIDO")]
    public void Refactor_Debe_Conservar_Visibilidad_Previa_Para_Rol_No_Reconocido(string? rol)
    {
        var solicitud = CrearSolicitud(1, 50, 5);

        Action validar = () => ReglaConsultaSolicitud.Validar(solicitud, 20, 99, rol, []);

        validar.Should().NotThrow();
    }

    private static Solicitud CrearSolicitud(long id, long? idArea, long? idPadre)
    {
        var solicitud = SolicitudMother.Borrador(id);
        var usuario = new Usuario(10, RolUsuario.SolicitanteAprobador);
        typeof(Usuario).GetField("<IdArea>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(usuario, idArea);
        if (idArea.HasValue)
        {
            var area = new Area();
            typeof(Area).GetField("<IdAreaPadre>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(area, idPadre);
            typeof(Usuario).GetProperty(nameof(Usuario.Area))!.SetValue(usuario, area);
        }
        typeof(Solicitud).GetProperty(nameof(Solicitud.UsuarioSolicitante))!.SetValue(solicitud, usuario);
        return solicitud;
    }
}
