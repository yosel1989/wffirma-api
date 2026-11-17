using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Solicitudes;

public class SolicitudVisualizacionPersistenceTest
{
    private static WorkflowFirmaDbContext Contexto(string nombre) => new(
        new DbContextOptionsBuilder<WorkflowFirmaDbContext>().UseInMemoryDatabase(nombre).Options, null!);

    [Fact]
    public async Task SaveChanges_PersisteLaVistaYElReinicioSinModificarLaAprobacion()
    {
        var nombre = Guid.NewGuid().ToString();
        var fecha = new DateTime(2026, 11, 17, 12, 0, 0);
        using (var contexto = Contexto(nombre))
        {
            contexto.Set<SolicitudFirmanteFlujo>().Add(SolicitudFirmanteFlujo.Crear(1, 2, 3, 4, 1, 100));
            await contexto.SaveChangesAsync();
        }
        using (var contexto = Contexto(nombre))
        {
            var firmante = await contexto.Set<SolicitudFirmanteFlujo>().SingleAsync();
            using (HoraSistema.Fijar(fecha))
                firmante.RegistrarVistaSolicitud();
            contexto.ChangeTracker.DetectChanges();
            Assert.Equal(new[] { nameof(SolicitudFirmanteFlujo.FechaVistaSolicitud) },
                contexto.Entry(firmante).Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name));
            await contexto.SaveChangesAsync();
        }
        using (var contexto = Contexto(nombre))
        {
            var firmante = await contexto.Set<SolicitudFirmanteFlujo>().SingleAsync();
            Assert.Equal(fecha, firmante.FechaVistaSolicitud);
            Assert.False(firmante.RevAprobado);
            Assert.Null(firmante.FechaRevision);
            firmante.ResetearPorRechazo();
            await contexto.SaveChangesAsync();
        }
        using var verificacion = Contexto(nombre);
        Assert.Null((await verificacion.Set<SolicitudFirmanteFlujo>().SingleAsync()).FechaVistaSolicitud);
    }

    [Fact]
    public async Task Repositorio_SoloObtieneFilasActivasPropiasSinExigirTurnoNiEstadoPendiente()
    {
        var nombre = Guid.NewGuid().ToString();
        using (var contexto = Contexto(nombre))
        {
            var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", true);
            var etapa = plantilla.AgregarEtapas(10, 2, "Etapa", TipoEtapa.Secuencial, 1, TipoAprobador.Firmante, 10);
            etapa.AgregarFirmante(1, 100, "Cargo", "GTI", "GTI", 10);
            etapa.AgregarFirmante(2, 200, "Cargo", "GTI", "GTI", 10);
            foreach (var id in new long[] { 8, 9 })
            {
                var solicitud = Solicitud.CrearBorrador(id, $"SOL-{id}", 1, 2, 10, 3, 10, 5, 2, "Contrato", null, false, false, null, 0);
                var flujo = SolicitudPlantilla.Crear(id, solicitud, plantilla);
                solicitud.AsignarPlantilla(flujo);
                flujo.AgregarFirmanteFlujo(id * 10, 1, 10, 2, 100);
                flujo.AgregarFirmanteFlujo(id * 10 + 1, 2, 10, 2, 200);
                flujo.AgregarFirmanteFlujo(id * 10 + 2, 1, 10, 2, 100);
                flujo.FirmantesFlujo.Single(f => f.Id == id * 10).Aprobar();
                flujo.FirmantesFlujo.Single(f => f.Id == id * 10 + 2).Inactivar();
                contexto.Solicitudes.Add(solicitud);
            }
            await contexto.SaveChangesAsync();
        }
        using (var contexto = Contexto(nombre))
        {
            var filas = await new SolicitudRepository(contexto).ObtenerFirmantesParaVisualizacionAsync(8, 100, default);
            var propia = Assert.Single(filas);
            Assert.Equal(80, propia.Id);
            Assert.True(propia.RevAprobado);
            propia.RegistrarVistaSolicitud();
            await contexto.SaveChangesAsync();
        }
        using var verificacion = Contexto(nombre);
        var registros = await verificacion.SolicitudFirmanteFlujos.ToListAsync();
        Assert.Equal(80, Assert.Single(registros.Where(f => f.FechaVistaSolicitud.HasValue)).Id);
        Assert.All(registros.Where(f => f.Id != 80), f => Assert.Null(f.FechaVistaSolicitud));
        Assert.Empty(await new SolicitudRepository(verificacion).ObtenerFirmantesParaVisualizacionAsync(8, 999, default));
    }

    [Fact]
    public void Oracle_MapeaColumnaNullableTimestampSinConectarseALaBase()
    {
        var opciones = new DbContextOptionsBuilder<WorkflowFirmaDbContext>().UseOracle("Data Source=localhost").Options;
        using var contexto = new WorkflowFirmaDbContext(opciones, null!);
        var entidad = contexto.Model.FindEntityType(typeof(SolicitudFirmanteFlujo))!;
        var propiedad = entidad.FindProperty(nameof(SolicitudFirmanteFlujo.FechaVistaSolicitud))!;
        var tabla = StoreObjectIdentifier.Table("WFF_SOLICITUD_FIRMANTE_FLUJO", "US_WFFIRMAS");
        Assert.True(propiedad.IsNullable);
        Assert.Equal("TS_FECHA_VISTA_SOLICITUD", propiedad.GetColumnName(tabla));
        Assert.Equal("TIMESTAMP", propiedad.GetColumnType());
        var sql = contexto.Set<SolicitudFirmanteFlujo>().Where(f => f.Id == 1).Select(f => f.FechaVistaSolicitud).ToQueryString();
        Assert.Contains("TS_FECHA_VISTA_SOLICITUD", sql);
    }
}
