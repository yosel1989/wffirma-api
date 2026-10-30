using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.AccesosEspeciales
{
    public class AccesoEspecialQueryRepositoryConsultasTests
    {
        private static WorkflowFirmaDbContext CrearDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new WorkflowFirmaDbContext(options, null!);
        }

        private static string NuevoDbName() => Guid.NewGuid().ToString();

        private static async Task SembrarAsync(string dbName, params AccesoEspecial[] accesos)
        {
            using var seed = CrearDbContext(dbName);
            seed.AccesosEspeciales.AddRange(accesos);
            await seed.SaveChangesAsync();
        }

        [Fact]
        public async Task ExistAsync_UsuarioYAreaExistentes_DevuelveTrue()
        {
            var dbName = NuevoDbName();
            await SembrarAsync(dbName, AccesoEspecial.Crear(100, 1, 10));

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialQueryRepository(context);

            var resultado = await repository.ExistAsync(1, 10, CancellationToken.None);

            Assert.True(resultado);
        }

        [Fact]
        public async Task ExistAsync_BaseVacia_DevuelveFalse()
        {
            using var context = CrearDbContext(NuevoDbName());
            var repository = new AccesoEspecialQueryRepository(context);

            var resultado = await repository.ExistAsync(1, 10, CancellationToken.None);

            Assert.False(resultado);
        }

        [Fact]
        public async Task ExistAsync_MismoUsuarioPeroOtraArea_DevuelveFalse()
        {
            var dbName = NuevoDbName();
            await SembrarAsync(dbName, AccesoEspecial.Crear(100, 1, 10));

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialQueryRepository(context);

            var resultado = await repository.ExistAsync(1, 99, CancellationToken.None);

            Assert.False(resultado);
        }

        [Fact]
        public async Task ExistAsync_MismaAreaPeroOtroUsuario_DevuelveFalse()
        {
            var dbName = NuevoDbName();
            await SembrarAsync(dbName, AccesoEspecial.Crear(100, 1, 10));

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialQueryRepository(context);

            var resultado = await repository.ExistAsync(99, 10, CancellationToken.None);

            Assert.False(resultado);
        }

        [Fact]
        public async Task ExistAsync_AccesoInactivo_DevuelveTrueIgnorandoFiltros()
        {
            // Ajusta esto a tu filtro global (Estado, Eliminado, etc.)
            var dbName = NuevoDbName();
            var acceso = AccesoEspecial.Crear(100, 1, 10);
            using (var seed = CrearDbContext(dbName))
            {
                seed.AccesosEspeciales.Add(acceso);
                seed.Entry(acceso).Property(a => a.Estado).CurrentValue = false;
                await seed.SaveChangesAsync();
            }

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialQueryRepository(context);

            var resultado = await repository.ExistAsync(1, 10, CancellationToken.None);

            Assert.True(resultado);
        }

        [Fact]
        public async Task ExistAsync_VariosAccesos_EncuentraElParCorrecto()
        {
            var dbName = NuevoDbName();
            await SembrarAsync(dbName,
                AccesoEspecial.Crear(100, 1, 10),
                AccesoEspecial.Crear(101, 2, 20));

            using var context = CrearDbContext(dbName);
            var repository = new AccesoEspecialQueryRepository(context);

            Assert.True(await repository.ExistAsync(2, 20, CancellationToken.None));
            Assert.False(await repository.ExistAsync(1, 20, CancellationToken.None)); // cruce de pares
        }

        [Fact]
        public async Task ExistAsync_TokenYaCancelado_LanzaOperationCanceledException()
        {
            using var context = CrearDbContext(NuevoDbName());
            var repository = new AccesoEspecialQueryRepository(context);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repository.ExistAsync(1, 10, cts.Token));
        }
    }
}
