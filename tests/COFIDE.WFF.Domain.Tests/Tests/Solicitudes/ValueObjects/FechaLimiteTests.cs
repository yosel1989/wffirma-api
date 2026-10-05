using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes.ValueObjects;

public sealed record FechaLimiteCase(
    string Caso, int MinutosDesdeAhora, bool EsValida, string? CodigoError)
{
    public override string ToString() => Caso;
}

public class FechaLimiteTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<FechaLimiteCase>("Solicitudes/fecha-limite.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void Crear_Solo_Debe_Aceptar_Fechas_Futuras(FechaLimiteCase caso)
    {
        var fecha = DateTime.Now.AddMinutes(caso.MinutosDesdeAhora);

        var act = () => FechaLimite.Crear(fecha);

        if (!caso.EsValida)
        {
            act.Should().Throw<DomainException>()
               .Which.Error.Code.Should().Be(caso.CodigoError);
            return;
        }

        act().Valor.Should().BeCloseTo(fecha, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EstaPorVencer_Debe_Comparar_Contra_El_Margen_Recibido()
    {
        var limite = FechaLimite.Crear(DateTime.Now.AddHours(2));

        limite.EstaVencida().Should().BeFalse();
        limite.EstaPorVencer(TimeSpan.FromHours(3)).Should().BeTrue();
        limite.EstaPorVencer(TimeSpan.FromMinutes(30)).Should().BeFalse();
    }

    [Fact]
    public void Dos_Fechas_Limite_Con_El_Mismo_Valor_Deben_Ser_Iguales()
    {
        var valor = DateTime.Now.AddDays(1);

        FechaLimite.Crear(valor).Should().Be(FechaLimite.Crear(valor));
    }
}
