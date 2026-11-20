using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using VO = COFIDE.WFF.Domain.ValueObjects;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Solicitudes;

public class ContarSolicitudesNoVistasTest
{
    private const long Primero = 100;
    private const long Segundo = 200;

    private static WorkflowFirmaDbContext Contexto(string nombre) => new(
        new DbContextOptionsBuilder<WorkflowFirmaDbContext>().UseInMemoryDatabase(nombre).Options, null!);

    [Fact]
    public async Task Conteo_SoloIncluyeSolicitudesEnCursoEnTurnoDelUsuarioSinFechaDeVista()
    {
        var nombre = await Sembrar();

        using var consulta = Contexto(nombre);
        var repositorio = new SolicitudQueryRepository(consulta, null!, TimeProvider.System);

        Assert.Equal(1, await repositorio.ContarSolicitudesPorAprobarNoVistas(Primero, default));
        Assert.Equal(1, await repositorio.ContarSolicitudesPorAprobarNoVistas(Segundo, default));
        Assert.Equal(0, await repositorio.ContarSolicitudesPorAprobarNoVistas(999, default));
    }

    [Fact]
    public async Task DestinatariosEnTurno_IncluyePendientesYaVistasYPreservaElTurno()
    {
        var nombre = await Sembrar();

        using var consulta = Contexto(nombre);
        var repositorio = new SolicitudQueryRepository(consulta, null!, TimeProvider.System);

        var primero = Assert.Single(await repositorio.ObtenerDestinatariosSolicitudPendienteFirmaAsync(1, default));
        Assert.Equal($"cuenta{Primero}", primero.CodigoUsuario);
        Assert.Equal(2, primero.TotalSolicitudesPendientesFirma);

        var segundo = Assert.Single(await repositorio.ObtenerDestinatariosSolicitudPendienteFirmaAsync(4, default));
        Assert.Equal($"cuenta{Segundo}", segundo.CodigoUsuario);
        Assert.Equal(1, segundo.TotalSolicitudesPendientesFirma);

        Assert.Empty(await repositorio.ObtenerDestinatariosSolicitudPendienteFirmaAsync(3, default));
    }

    private static async Task<string> Sembrar()
    {
        var nombre = Guid.NewGuid().ToString();
        using (var contexto = Contexto(nombre))
        {
            foreach (var id in new[] { Primero, Segundo })
            {
                var usuario = new Usuario(id, RolUsuario.SolicitanteAprobador);
                typeof(Usuario).GetProperty(nameof(Usuario.CuentaLogueo))!.SetValue(usuario, $"cuenta{id}");
                foreach (var campo in new[] { nameof(Usuario.Nombres), nameof(Usuario.ApellidoPaterno), nameof(Usuario.ApellidoMaterno), nameof(Usuario.NumeroDocumento), nameof(Usuario.Email) })
                    typeof(Usuario).GetProperty(campo)!.SetValue(usuario, $"{campo}{id}");
                usuario.Activar();
                contexto.Usuarios.Add(usuario);
            }

            var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", true);
            plantilla.AgregarEtapas(10, 1, "Etapa 1", TipoEtapa.Secuencial, 1, TipoAprobador.Firmante, 10)
                .AgregarFirmante(1, Primero, "Cargo", "GTI", "GTI", 10);
            plantilla.AgregarEtapas(11, 2, "Etapa 2", TipoEtapa.Secuencial, 1, TipoAprobador.Firmante, 10)
                .AgregarFirmante(2, Segundo, "Cargo", "GTI", "GTI", 10);

            // 1: turno del primero, sin ver. 2: turno del primero, ya vista. 3: borrador.
            // 4: el primero aprobo, turno del segundo sin ver.
            contexto.Solicitudes.AddRange(
                Crear(1, plantilla, enCurso: true),
                Crear(2, plantilla, enCurso: true, primeroVio: true),
                Crear(3, plantilla, enCurso: false),
                Crear(4, plantilla, enCurso: true, primeroAprobo: true));
            await contexto.SaveChangesAsync();
        }
        return nombre;
    }

    private static Solicitud Crear(long id, Plantilla plantilla, bool enCurso, bool primeroVio = false, bool primeroAprobo = false)
    {
        var solicitud = Solicitud.CrearBorrador(id, $"SOL-{id}", 1, 2, 10, 3, 10, 5, 2, "Contrato", null, false, false, null, 0);
        if (enCurso)
            typeof(Solicitud).GetProperty(nameof(Solicitud.IdEstadoSolicitud))!
                .SetValue(solicitud, Convert.ToInt64(VO.EstadoSolicitud.EnCurso.Codigo));

        var flujo = SolicitudPlantilla.Crear(id, solicitud, plantilla);
        solicitud.AsignarPlantilla(flujo);
        flujo.AgregarFirmanteFlujo(id * 10, 1, 10, 1, Primero);
        flujo.AgregarFirmanteFlujo(id * 10 + 1, 2, 11, 2, Segundo);

        var primero = flujo.FirmantesFlujo.Single(f => f.Id == id * 10);
        if (primeroVio)
            primero.RegistrarVistaSolicitud();
        if (primeroAprobo)
            primero.Aprobar();
        return solicitud;
    }
}
