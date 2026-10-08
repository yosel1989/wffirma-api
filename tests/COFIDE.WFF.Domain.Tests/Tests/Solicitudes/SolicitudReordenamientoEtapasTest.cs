using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

public class SolicitudReordenamientoEtapasTest
{
    [Fact]
    public void Sin_Etapas_Bloqueadas_Debe_Permitir_Agregar_Desde_La_Primera_Posicion()
    {
        var solicitud = SolicitudMother.Borrador();

        var act = () => solicitud.ValidarReordenamientoEtapas([1, 2]);

        act.Should().NotThrow();
    }

    [Fact]
    public void Posicion_Igual_O_Anterior_A_La_Ultima_Etapa_Bloqueada_Debe_Fallar()
    {
        var solicitud = SolicitudMother.Borrador();

        var act = () => solicitud.ValidarReordenamientoEtapas([0]);

        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(SolicitudErrors.CrearEtapaPrevia);
    }
}
