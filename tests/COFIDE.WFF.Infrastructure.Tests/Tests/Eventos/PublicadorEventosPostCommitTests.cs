using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Events;
using COFIDE.WFF.Infrastructure.Eventos;
using COFIDE.WFF.Infrastructure.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Eventos;

/// <summary>B1 (híbrido): publicación inmediata como antes; si falla, a la cola de reintentos sin error para el usuario.</summary>
public class PublicadorEventosPostCommitTests
{
    private static readonly PublicacionEventosOptions Opciones = new() { MaximoReintentos = 3, SegundosEntreReintentos = 10, CapacidadCola = 10 };

    [Fact]
    public async Task Si_La_Publicacion_Funciona_Debe_Publicar_De_Inmediato_Y_No_Encolar()
    {
        var dispatcher = new DispatcherDePrueba();
        var cola = new ColaReintentoEventos(Options.Create(Opciones));
        var evento = new SolicitudAprobadaDomainEvent(8, "Luis");

        await Crear(dispatcher, cola).PublicarAsync([evento], CancellationToken.None);

        dispatcher.Publicados.Should().ContainSingle().Which.Should().BeSameAs(evento);
        cola.Lector.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Si_La_Publicacion_Falla_No_Debe_Lanzar_Y_Debe_Encolar_El_Reintento()
    {
        var dispatcher = new DispatcherDePrueba(fallosIniciales: 1);
        var cola = new ColaReintentoEventos(Options.Create(Opciones));
        var evento = new SolicitudAprobadaDomainEvent(8, "Luis");

        var act = () => Crear(dispatcher, cola).PublicarAsync([evento], CancellationToken.None);

        await act.Should().NotThrowAsync("la transacción ya se confirmó; el usuario no debe recibir error");
        cola.Lector.TryRead(out var pendiente).Should().BeTrue();
        pendiente!.Evento.Should().BeSameAs(evento);
        pendiente.Intento.Should().Be(1);
        pendiente.ReintentarDesde.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Un_Evento_Que_Falla_No_Debe_Impedir_Publicar_Los_Demas()
    {
        var falla = new SolicitudRechazadaDomainEvent(8, "Luis");
        var dispatcher = new DispatcherDePrueba(fallaPara: e => ReferenceEquals(e, falla));
        var cola = new ColaReintentoEventos(Options.Create(Opciones));
        var ok1 = new SolicitudAprobadaDomainEvent(8, "Luis");
        var ok2 = new SolicitudPorAprobarFirmantesDomainEvent(8);

        await Crear(dispatcher, cola).PublicarAsync([ok1, falla, ok2], CancellationToken.None);

        dispatcher.Publicados.Should().Equal(ok1, ok2);
        cola.Lector.TryRead(out var pendiente).Should().BeTrue();
        pendiente!.Evento.Should().BeSameAs(falla);
    }

    [Fact]
    public async Task Sin_Reintentos_Configurados_No_Debe_Encolar()
    {
        var opciones = new PublicacionEventosOptions { MaximoReintentos = 0 };
        var dispatcher = new DispatcherDePrueba(fallosIniciales: 1);
        var cola = new ColaReintentoEventos(Options.Create(opciones));

        await new PublicadorEventosPostCommit(dispatcher, cola, Options.Create(opciones), NullLogger<PublicadorEventosPostCommit>.Instance)
            .PublicarAsync([new SolicitudAprobadaDomainEvent(8, "Luis")], CancellationToken.None);

        cola.Lector.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task Cola_Llena_No_Debe_Lanzar()
    {
        var opciones = new PublicacionEventosOptions { MaximoReintentos = 3, CapacidadCola = 1 };
        var dispatcher = new DispatcherDePrueba(fallosIniciales: 10);
        var cola = new ColaReintentoEventos(Options.Create(opciones));
        var publicador = new PublicadorEventosPostCommit(dispatcher, cola, Options.Create(opciones), NullLogger<PublicadorEventosPostCommit>.Instance);

        var act = () => publicador.PublicarAsync(
            [new SolicitudAprobadaDomainEvent(1, "A"), new SolicitudAprobadaDomainEvent(2, "B")],
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static PublicadorEventosPostCommit Crear(DispatcherDePrueba dispatcher, ColaReintentoEventos cola) =>
        new(dispatcher, cola, Options.Create(Opciones), NullLogger<PublicadorEventosPostCommit>.Instance);
}
