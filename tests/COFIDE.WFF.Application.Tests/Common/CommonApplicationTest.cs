using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Application.Common.Results;
using COFIDE.WFF.Domain.Common.Abstractions;
using FluentAssertions;
using MediatR;
using Moq;

namespace COFIDE.WFF.Application.Tests.Common;

public class CommonApplicationTest
{
    [Fact]
    public void Result_Ok_Y_Fail_Deben_Conservar_Codigo_Y_Datos()
    {
        var ok = Result<string>.Ok("listo", "guardado");
        var fail = Result<string>.Fail("APP_SOL_001", "inválida", ["detalle"]);

        ok.Success.Should().BeTrue();
        ok.Code.Should().Be("200");
        ok.Data.Should().Be("listo");
        ok.Message.Should().Be("guardado");
        fail.Success.Should().BeFalse();
        fail.Code.Should().Be("APP_SOL_001");
        fail.Data.Should().BeNull();
        fail.Errors.Should().ContainSingle("detalle");
    }

    [Fact]
    public void ApplicationError_Format_Debe_Reemplazar_El_Marcador()
    {
        var error = ApplicationErrors.FirmaDocumentoInvalid.Format("dni distinto");

        error.Code.Should().Be("APP_SOL_007");
        error.Description.Should().Be("Firma Documento: dni distinto.");
    }

    [Fact]
    public async Task Dispatcher_Debe_Publicar_Solo_El_Momento_Pedido()
    {
        var publicados = new List<INotification>();
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((notificacion, _) => publicados.Add(notificacion))
            .Returns(Task.CompletedTask);
        var dispatcher = new DomainEventDispatcher(mediator.Object);
        var previo = new EventoPrevio();
        var posterior = new EventoPosterior();

        await dispatcher.DispatchPreCommitAsync([previo, posterior], CancellationToken.None);
        await dispatcher.DispatchPostCommitAsync([previo, posterior], CancellationToken.None);
        await dispatcher.DispatchAsync([previo], CancellationToken.None);

        publicados.Should().HaveCount(3);
        publicados[0].Should().BeOfType<DomainEventNotification<EventoPrevio>>();
        publicados[1].Should().BeOfType<DomainEventNotification<EventoPosterior>>();
        publicados[2].Should().BeOfType<DomainEventNotification<EventoPrevio>>();
    }

    public sealed record EventoPrevio : IPreCommitDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public sealed record EventoPosterior : IPostCommitDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
