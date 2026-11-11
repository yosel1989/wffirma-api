using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales;
using FluentAssertions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.AccesosEspeciales
{
    public class AccesoEspecialRepositoryTests
    {
        private static WorkflowFirmaDbContext CrearDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new WorkflowFirmaDbContext(options, null!);
        }

        private static string NuevoDbName() => Guid.NewGuid().ToString();

        // ---------- Registrar ----------

        [Fact]
        public async Task RegistrarAccesoEspecialAsync_PersisteElAcceso()
        {
            // Arrange
            var dbName = NuevoDbName();
            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialRepository(context);
            var acceso = AccesoEspecial.Crear(100, 1, 10);

            // Act
            repository.RegistrarAccesoEspecialAsync(acceso);

            // Assert (contexto nuevo: lee lo realmente guardado)
            using var verificacion = CrearDbContext(dbName);
            var guardado = await verificacion.AccesosEspeciales
                .IgnoreQueryFilters()
                .SingleAsync(x => x.Id == 100);

            Assert.Equal(1, guardado.IdUsuario);
            Assert.Equal(10, guardado.IdArea);
        }

        // ---------- GetByIdAnyStatusAsync ----------

        [Fact]
        public async Task GetByIdAnyStatusAsync_IdExistente_DevuelveElAcceso()
        {
            var dbName = NuevoDbName();
            using (var seed = CrearDbContext(dbName))
            {
                seed.AccesosEspeciales.Add(AccesoEspecial.Crear(100, 1, 10));
                await seed.SaveChangesAsync();
            }

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialRepository(context);

            var resultado = await repository.GetByIdAnyStatusAsync(100, tracking: false);

            Assert.NotNull(resultado);
            Assert.Equal(100, resultado!.Id);
        }

        [Fact]
        public async Task GetByIdAnyStatusAsync_IdInexistente_DevuelveNull()
        {
            using var context = CrearDbContext(NuevoDbName());
            var repository = new AccesoEspecialRepository(context);

            var resultado = await repository.GetByIdAnyStatusAsync(999, tracking: false);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task GetByIdAnyStatusAsync_ConTrackingTrue_LaEntidadQuedaRastreada()
        {
            var dbName = NuevoDbName();
            using (var seed = CrearDbContext(dbName))
            {
                seed.AccesosEspeciales.Add(AccesoEspecial.Crear(100, 1, 10));
                await seed.SaveChangesAsync();
            }

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialRepository(context);

            var resultado = await repository.GetByIdAnyStatusAsync(100, tracking: true);

            Assert.Equal(EntityState.Unchanged, context.Entry(resultado!).State);
        }

        [Fact]
        public async Task GetByIdAnyStatusAsync_ConTrackingFalse_LaEntidadNoQuedaRastreada()
        {
            var dbName = NuevoDbName();
            using (var seed = CrearDbContext(dbName))
            {
                seed.AccesosEspeciales.Add(AccesoEspecial.Crear(100, 1, 10));
                await seed.SaveChangesAsync();
            }

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialRepository(context);

            var resultado = await repository.GetByIdAnyStatusAsync(100, tracking: false);

            Assert.Equal(EntityState.Detached, context.Entry(resultado!).State);
        }

        [Fact]
        public async Task GetByIdAnyStatusAsync_AccesoInactivo_LoDevuelveIgnorandoFiltros()
        {
            // Ajusta esto a tu filtro global (Estado, Eliminado, etc.)
            var dbName = NuevoDbName();
            using (var seed = CrearDbContext(dbName))
            {
                var acceso = AccesoEspecial.Crear(100, 1, 10);
                seed.AccesosEspeciales.Add(acceso);
                seed.Entry(acceso).Property(a => a.Estado).CurrentValue = false;
                await seed.SaveChangesAsync();
            }

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialRepository(context);

            var resultado = await repository.GetByIdAnyStatusAsync(100, tracking: false);

            Assert.NotNull(resultado);
        }

        [Fact]
        public async Task GetAreasByUserIdAsync_SoloDevuelveActivos_YReflejaLaReactivacion()
        {
            var dbName = NuevoDbName();
            using (var seed = CrearDbContext(dbName))
            {
                var activo = AccesoEspecial.Crear(100, 7, 10);
                var inactivo = AccesoEspecial.Crear(101, 7, 20);
                inactivo.Desactivar();
                var eliminado = AccesoEspecial.Crear(102, 7, 30);
                eliminado.Eliminar();
                seed.AccesosEspeciales.AddRange(activo, inactivo, eliminado);
                await seed.SaveChangesAsync();
            }

            using (var contexto = CrearDbContext(dbName))
            {
                var repository = new AccesoEspecialRepository(contexto);

                (await repository.GetAreasByUserIdAsync(7)).Should().Equal(10);

                var accesoReactivado = await repository.GetByIdAnyStatusAsync(101, tracking: true);
                accesoReactivado!.Activar();
                await contexto.SaveChangesAsync();
            }

            using var verificacion = CrearDbContext(dbName);
            var areasActivas = await new AccesoEspecialRepository(verificacion).GetAreasByUserIdAsync(7);
            areasActivas.Should().Equal(10, 20);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_MarcaLaEntidadComoEliminada_YSeBorraAlGuardar()
        {
            var dbName = NuevoDbName();
            using var context = CrearDbContext(dbName);
            var acceso = AccesoEspecial.Crear(100, 1, 10);
            context.AccesosEspeciales.Add(acceso);
            await context.SaveChangesAsync();

            var repository = new AccesoEspecialRepository(context);

            repository.Delete(acceso);

            // Delete solo marca; aún no se ejecuta en la BD
            Assert.Equal(EntityState.Deleted, context.Entry(acceso).State);

            await context.SaveChangesAsync();

            using var verificacion = CrearDbContext(dbName);
            Assert.False(await verificacion.AccesosEspeciales
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id == 100));
        }
    }
}
