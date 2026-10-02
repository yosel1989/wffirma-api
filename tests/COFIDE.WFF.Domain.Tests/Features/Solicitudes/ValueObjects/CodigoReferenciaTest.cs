using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Common.TestSupport;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Solicitudes.ValueObjects;

public sealed record CodigoReferenciaCase(
    string Caso, string? Entrada, int LongitudExtra, string? Esperado, string? CodigoError)
{
    public override string ToString() => Caso;
}

public class CodigoReferenciaTest
{
    public static IEnumerable<object[]> Casos =>
    JsonResource.Cases<CodigoReferenciaCase>("Solicitudes/codigo-referencia.json");


    [Theory]
    [MemberData(nameof(Casos))]
    public void Crear_Debe_Exigir_Valor_Y_Respetar_La_Longitud_Maxima(CodigoReferenciaCase caso)
    {
        // LongitudExtra > construye una cadena que excede MaxLongitud sin fijarla en el JSON. var entrada = caso. Longitud Extra > >
        var entrada = caso.LongitudExtra > 0
        ? new string('X', CodigoReferencia.MaxLongitud + caso.LongitudExtra)
            : caso.Entrada;
        var act = () => CodigoReferencia.Crear(entrada);
        if (caso.CodigoError is not null)
        {
            act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(caso.CodigoError);
            return;
        }

        act().Valor.Should().Be(caso.Esperado);
    }

    [Fact]
    public void Crear_En_El_Limite_Exacto_Debe_Ser_Valido()
    {
        var limite = new string('X', CodigoReferencia.MaxLongitud);

        CodigoReferencia.Crear(limite).Valor.Should().HaveLength(CodigoReferencia.MaxLongitud);
    }
}
