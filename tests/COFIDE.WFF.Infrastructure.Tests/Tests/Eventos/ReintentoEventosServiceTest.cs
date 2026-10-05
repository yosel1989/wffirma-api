using COFIDE.WFF.Application.Common.Events;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Infrastructure.Eventos;
using COFIDE.WFF.Infrastructure.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Eventos;

/// <summary>B1 (híbrido): reintentos en segundo plano de los eventos que fallaron al publicarse.</summary>
public class ReintentoEventosServiceTest
{
    private static readonly PublicacionEventosOptions Opciones = new() { MaximoReintentos = 3, SegundosEntreReintentos = 0, CapacidadCola = 10 };

    [Fact]
    public async Task Reintento_Exitoso_Debe_Publicar_Y_No_Volver_A_Encolar()
    {
        var dispatcher = new DispatcherDePrueba();
        var (servicio, cola) = Crear(dispatcher);
        var evento = new SolicitudAprobadaDomainEvent(8, "Luis");

        await servicio.ProcesarAsync(new EventoPendiente(evento, 1, DateTime.UtcNow), CancellationToken.None);

        dispatcher.Publicados.Should().ContainSingle().Which.Should().BeSameAs(evento);
        cola.Lector.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Reintento_Fallido_Debe_Volver_A_Encolar_Con_El_Siguiente_Intento()
    {
        var dispatcher = new DispatcherDePrueba(fallosIniciales: 1);
        var (servicio, cola) = Crear(dispatcher);
        var evento = new SolicitudAprobadaDomainEvent(8, "Luis");

        await servicio.ProcesarAsync(new EventoPendiente(evento, 1, DateTime.UtcNow), CancellationToken.None);

        cola.Lector.TryRead(out var siguiente).Should().BeTrue();
        siguiente!.Intento.Should().Be(2);
        siguiente.Evento.Should().BeSameAs(evento);
    }

    [Fact]
    public async Task Al_Agotar_Los_Reintentos_No_Debe_Volver_A_Encolar()
    {
        var dispatcher = new DispatcherDePrueba(fallosIniciales: 10);
        var (servicio, cola) = Crear(dispatcher);

        await servicio.ProcesarAsync(
            new EventoPendiente(new SolicitudAprobadaDomainEvent(8, "Luis"), Opciones.MaximoReintentos, DateTime.UtcNow),
            CancellationToken.None);

        cola.Lector.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Servicio_En_Ejecucion_Debe_Procesar_Lo_Que_Llega_A_La_Cola()
    {
        var dispatcher = new DispatcherDePrueba();
        var (servicio, cola) = Crear(dispatcher);
        var evento = new SolicitudAprobadaDomainEvent(8, "Luis");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await servicio.StartAsync(cts.Token);
        cola.Encolar(new EventoPendiente(evento, 1, DateTime.UtcNow)).Should().BeTrue();

        while (dispatcher.Publicados.Count == 0 && !cts.IsCancellationRequested)
            await Task.Delay(20);
        await servicio.StopAsync(CancellationToken.None);

        dispatcher.Publicados.Should().ContainSingle().Which.Should().BeSameAs(evento);
    }

    private static (ReintentoEventosService Servicio, ColaReintentoEventos Cola) Crear(DispatcherDePrueba dispatcher)
    {
        var cola = new ColaReintentoEventos(Options.Create(Opciones));
        var proveedor = new ServiceCollection()
            .AddSingleton<IDomainEventDispatcher>(dispatcher)
            .BuildServiceProvider();

        var servicio = new ReintentoEventosService(
            cola,
            proveedor.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(Opciones),
            NullLogger<ReintentoEventosService>.Instance);

        return (servicio, cola);
    }
}
