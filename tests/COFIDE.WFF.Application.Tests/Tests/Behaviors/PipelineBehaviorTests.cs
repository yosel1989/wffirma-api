using System.Reflection;
using COFIDE.WFF.Application.Common.Behaviors;
using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Common.Abstractions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Tests.Behaviors;

public class PipelineBehaviorTest
{
    [Fact]
    public async Task Validacion_Sin_Validadores_Debe_Continuar()
    {
        var behavior = new ValidationBehavior<DatoCommand, string>([]);
        var ejecutado = false;

        var resultado = await behavior.Handle(
            new DatoCommand(null),
            _ =>
            {
                ejecutado = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        resultado.Should().Be("ok");
        ejecutado.Should().BeTrue();
    }

    [Fact]
    public async Task Validacion_Con_Errores_Debe_Lanzar_Y_No_Continuar()
    {
        var behavior = new ValidationBehavior<DatoCommand, string>([new DatoValidator()]);
        var ejecutado = false;

        var act = () => behavior.Handle(
            new DatoCommand(null),
            _ =>
            {
                ejecutado = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        ejecutado.Should().BeFalse();
    }

    [Fact]
    public async Task Validacion_Correcta_Debe_Continuar()
    {
        var behavior = new ValidationBehavior<DatoCommand, string>([new DatoValidator()]);

        var resultado = await behavior.Handle(
            new DatoCommand("contrato"),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        resultado.Should().Be("ok");
    }

    [Fact]
    public async Task Transaccion_En_Una_Consulta_No_Debe_Abrir_Transaccion()
    {
        var uow = Unidad();
        var behavior = Transaccion<ConsultaPrueba>(uow);

        var resultado = await behavior.Handle(
            new ConsultaPrueba(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        resultado.Should().Be("ok");
        uow.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task Transaccion_En_Un_Comando_Debe_Confirmar_Y_Publicar_Eventos_PostCommit()
    {
        var uow = Unidad();
        var dispatcher = new Mock<IDomainEventDispatcher>();
        var publicador = new Mock<IPublicadorEventosPostCommit>();
        var archivos = new Mock<IRegistroArchivosTransaccion>();
        var behavior = Transaccion<GuardarCommand>(uow, dispatcher, publicador, archivos);
        dispatcher.Setup(d => d.DispatchPreCommitAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await behavior.Handle(
            new GuardarCommand(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        resultado.Should().Be("ok");
        uow.Verify(u => u.BeginTransactionAsync(), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.RollbackTransactionAsync(), Times.Never);
        dispatcher.Verify(d => d.DispatchPreCommitAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
        // B1: los eventos post-commit los publica el publicador (inmediato + reintentos si falla).
        publicador.Verify(p => p.PublicarAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
        archivos.Verify(a => a.ConfirmarArchivos(), Times.Once);
        archivos.Verify(a => a.DescartarArchivosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Transaccion_Debe_Publicar_Solo_Los_Eventos_PostCommit_Despues_Del_Commit()
    {
        var uow = Unidad();
        var pre = new EventoPreCommitPrueba();
        var post = new EventoPostCommitPrueba();
        uow.Setup(u => u.GetDomainEvents()).Returns(new IDomainEvent[] { pre, post });
        var orden = new List<string>();
        uow.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => orden.Add("commit")).Returns(Task.CompletedTask);
        var publicador = new Mock<IPublicadorEventosPostCommit>();
        IReadOnlyCollection<IDomainEvent>? publicados = null;
        publicador.Setup(p => p.PublicarAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback((IReadOnlyCollection<IDomainEvent> e, CancellationToken _) => { publicados = e; orden.Add("publicar"); })
            .Returns(Task.CompletedTask);

        await Transaccion<GuardarCommand>(uow, publicador: publicador)
            .Handle(new GuardarCommand(), _ => Task.FromResult("ok"), CancellationToken.None);

        publicados.Should().ContainSingle().Which.Should().BeSameAs(post);
        orden.Should().Equal("commit", "publicar");
    }

    [Fact]
    public async Task Transaccion_Cuando_Falla_Debe_Revertir_Y_Relanzar()
    {
        var uow = Unidad();
        var archivos = new Mock<IRegistroArchivosTransaccion>();
        var publicador = new Mock<IPublicadorEventosPostCommit>();
        var behavior = Transaccion<GuardarCommand>(uow, publicador: publicador, archivos: archivos);

        var act = () => behavior.Handle(
            new GuardarCommand(),
            _ => throw new InvalidOperationException("fallo"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        uow.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        uow.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        // B1: se compensan los archivos escritos y no se publica ningún evento.
        archivos.Verify(a => a.DescartarArchivosAsync(It.IsAny<CancellationToken>()), Times.Once);
        archivos.Verify(a => a.ConfirmarArchivos(), Times.Never);
        publicador.Verify(p => p.PublicarAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Auditoria_Debe_Guardar_Usuario_Y_Terminal_En_El_Comando()
    {
        var usuario = new Mock<ICurrentUserService>();
        usuario.Setup(u => u.CodigoUsuario).Returns("USR01");
        usuario.Setup(u => u.IpAddress).Returns("10.0.0.8");
        var comando = new AuditoriaCommand();

        await new AuditingBehavior<AuditoriaCommand, string>(usuario.Object)
            .Handle(comando, _ => Task.FromResult("ok"), CancellationToken.None);

        Leer(comando, "AuditUserName").Should().Be("USR01");
        Leer(comando, "AuditTerminal").Should().Be("10.0.0.8");
    }

    [Fact]
    public async Task Auditoria_En_Una_Consulta_Debe_Continuar_Sin_Tocar_Al_Usuario()
    {
        var usuario = new Mock<ICurrentUserService>();

        var resultado = await new AuditingBehavior<ConsultaPrueba, string>(usuario.Object)
            .Handle(new ConsultaPrueba(), _ => Task.FromResult("ok"), CancellationToken.None);

        resultado.Should().Be("ok");
        usuario.Verify(u => u.CodigoUsuario, Times.Never);
    }

    private static Mock<IUnitOfWork> Unidad()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        uow.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        uow.Setup(u => u.GetDomainEvents()).Returns(Array.Empty<IDomainEvent>());
        return uow;
    }

    private static TransactionBehavior<TRequest, string> Transaccion<TRequest>(
        Mock<IUnitOfWork> uow,
        Mock<IDomainEventDispatcher>? dispatcher = null,
        Mock<IPublicadorEventosPostCommit>? publicador = null,
        Mock<IRegistroArchivosTransaccion>? archivos = null)
        where TRequest : notnull =>
        new(
            uow.Object,
            (dispatcher ?? new Mock<IDomainEventDispatcher>()).Object,
            (publicador ?? new Mock<IPublicadorEventosPostCommit>()).Object,
            (archivos ?? new Mock<IRegistroArchivosTransaccion>()).Object,
            NullLogger<TransactionBehavior<TRequest, string>>.Instance);

    private sealed record EventoPreCommitPrueba : COFIDE.WFF.Domain.Common.Events.DomainEvent, IPreCommitDomainEvent;

    private sealed record EventoPostCommitPrueba : COFIDE.WFF.Domain.Common.Events.DomainEvent, IPostCommitDomainEvent;

    private static string? Leer(object destino, string propiedad)
    {
        for (var tipo = destino.GetType(); tipo != null; tipo = tipo.BaseType)
        {
            var miembro = tipo.GetProperty(propiedad, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (miembro != null)
                return miembro.GetValue(destino) as string;
        }

        return null;
    }

    private sealed record DatoCommand(string? Nombre) : IRequest<string>;

    private sealed class DatoValidator : AbstractValidator<DatoCommand>
    {
        public DatoValidator() => RuleFor(x => x.Nombre).NotEmpty();
    }

    private sealed record ConsultaPrueba : IRequest<string>;

    private sealed record GuardarCommand : IRequest<string>;

    private sealed record AuditoriaCommand : AuditableCommand<string>;
}
