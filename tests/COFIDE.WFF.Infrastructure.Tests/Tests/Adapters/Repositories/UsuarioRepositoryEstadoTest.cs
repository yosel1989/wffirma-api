using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public sealed class UsuarioRepositoryEstadoTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAnyStatusAsync_IncluyeAmbosEstadosYRespetaTracking(bool estado)
    {
        await using var context = CrearContexto();
        context.Usuarios.Add(new Usuario(7, RolUsuario.SolicitanteAprobador) { Estado = estado });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var repository = new UsuarioRepository(context);

        var sinTracking = await repository.GetByIdAnyStatusAsync(7, false, default);
        Assert.NotNull(sinTracking);
        Assert.Equal(7L, sinTracking.Id);
        Assert.Equal(estado, sinTracking.Estado);
        Assert.Empty(context.ChangeTracker.Entries<Usuario>());

        var conTracking = await repository.GetByIdAnyStatusAsync(7, true, default);
        Assert.NotNull(conTracking);
        Assert.Equal(estado, conTracking.Estado);
        Assert.Same(conTracking, Assert.Single(context.ChangeTracker.Entries<Usuario>()).Entity);
        Assert.Null(await repository.GetByIdAnyStatusAsync(999, false, default));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task UpdateAsync_UnitOfWork_PersisteEstadoYAuditoria(bool inicial, bool solicitado)
    {
        await using var context = CrearContexto();
        context.Usuarios.Add(new Usuario(7, RolUsuario.SolicitanteAprobador) { Estado = inicial });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        IUsuarioRepository repository = new UsuarioRepository(context);
        using var unitOfWork = new UnitOfWork(context);
        await unitOfWork.BeginTransactionAsync();
        var usuario = await repository.GetByIdAnyStatusAsync(7, true, default);
        Assert.NotNull(usuario);

        if (solicitado)
            usuario.Activar();
        else
            usuario.Desactivar();

        await repository.UpdateAsync(usuario);
        await unitOfWork.CommitTransactionAsync();
        context.ChangeTracker.Clear();
        var persistido = await context.Usuarios.AsNoTracking().SingleAsync(x => x.Id == 7);

        Assert.Equal(solicitado, persistido.Estado);
        Assert.Equal("SYSTEM", persistido.UsuarioModificacion);
        Assert.Equal("::1", persistido.TerminalModificacion);
        Assert.NotNull(persistido.FechaModificacion);
        Assert.False(unitOfWork.HasActiveTransaction);
    }

    [Fact]
    public async Task GetByIdAnyStatusAsync_Cancelado_PropagaCancelacion()
    {
        await using var context = CrearContexto();
        var repository = new UsuarioRepository(context);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repository.GetByIdAnyStatusAsync(7, true, cancellation.Token));
    }

    private static WorkflowFirmaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), options => options.EnableNullChecks(false))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new WorkflowFirmaDbContext(options, null!);
    }
}
