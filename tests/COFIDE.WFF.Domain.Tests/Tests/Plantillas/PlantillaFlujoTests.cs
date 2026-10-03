using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;


public sealed record OrdenEtapasCase(string Caso, List<long> Ordenes, string? CodigoError)
{
    public override string ToString() => Caso;
}

public class PlantillaFlujoTests
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<OrdenEtapasCase>("Plantillas/orden-etapas.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void ValidarFlujoCompleto_Debe_Exigir_Una_Secuencia_Desde_1_Sin_Saltos(OrdenEtapasCase caso)
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales([.. caso.Ordenes]);

        var act = plantilla.ValidarFlujoCompleto;

        if (caso.CodigoError is not null)
        {
            act.Should().Throw<DomainException>()
               .Which.Error.Code.Should().Be(caso.CodigoError);
            return;
        }

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidarFlujoCompleto_Sin_Etapas_Debe_Lanzar_DOM_PLT_010()
    {
        var plantilla = PlantillaMother.Vacia();

        var act = plantilla.ValidarFlujoCompleto;

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_010");
    }

    [Fact]
    public void ValidarFlujoCompleto_Con_Etapa_Sin_Firmantes_Debe_Lanzar_DOM_PLT_024()
    {
        var plantilla = PlantillaMother.Vacia();
        plantilla.AgregarEtapas(1, 1, "Etapa 1",
            TipoEtapa.Secuencial, 1,
            TipoAprobador.Firmante, 10);

        var act = plantilla.ValidarFlujoCompleto;

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_024");
    }

    [Fact]
    public void ValidarTotalFirmantes_Con_Mas_De_Veinte_Debe_Lanzar_DOM_PLT_008()
    {
        var plantilla = PlantillaMother.Vacia();
        var etapa = plantilla.AgregarEtapas(1, 1, "Etapa 1",
            TipoEtapa.Paralelo, 2,
            TipoAprobador.Firmante, 10);

        for (long i = 1; i <= 21; i++)
            etapa.AgregarFirmante(i, 100 + i, null, null, null, 10);

        var act = plantilla.ValidarTotalFirmantes;

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_008");
    }

    [Fact]
    public void Crear_Publica_No_Reutilizable_Debe_Lanzar_DOM_PLT_025()
    {
        var act = () => PlantillaMother.Vacia(publica: true, reutilizable: false);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_025");
    }

    [Fact]
    public void Crear_Con_Id_No_Positivo_Debe_Lanzar_DOM_PLT_001()
    {
        var act = () => PlantillaMother.Vacia(id: 0);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_001");
    }

    [Fact]
    public void AgregarFirmante_Con_Usuario_No_Positivo_Debe_Lanzar_DOM_PLT_007()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1);
        var etapa = plantilla.Etapas.Single();

        var act = () => etapa.AgregarFirmante(99, idUsuarioAsignado: 0, null, null, null, 10);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_007");
    }
}
