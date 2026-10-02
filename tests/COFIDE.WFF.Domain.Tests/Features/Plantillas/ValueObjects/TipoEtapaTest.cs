using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Common.TestSupport;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Plantillas.ValueObjects;

public sealed record TipoEtapaCase(
    string Caso, string TipoEtapa, int CantidadFirmantes, string? CodigoError)
{
    public override string ToString() => Caso;
}

public class TipoEtapaTest
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<TipoEtapaCase>("Plantillas/tipo-etapa-firmantes.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void ValidarCantidadFirmantes_Debe_Aplicar_La_Regla_Del_Tipo(TipoEtapaCase caso)
    {
        var tipo = caso.TipoEtapa == "SECUENCIAL" ? TipoEtapa.Secuencial : TipoEtapa.Paralelo;

        var act = () => tipo.ValidarCantidadFirmantes(caso.CantidadFirmantes);

        if (caso.CodigoError is not null)
        {
            act.Should().Throw<DomainException>()
               .Which.Error.Code.Should().Be(caso.CodigoError);
            return;
        }

        act.Should().NotThrow();
    }

    [Fact]
    public void PermiteMultiplesFirmantes_Solo_Debe_Ser_Verdadero_En_Paralelo()
    {
        TipoEtapa.Paralelo.PermiteMultiplesFirmantes().Should().BeTrue();
        TipoEtapa.Secuencial.PermiteMultiplesFirmantes().Should().BeFalse();
    }

    [Fact]
    public void ObtenerTipoEtapa_Sin_Clave_En_El_Diccionario_Debe_Lanzar_DOM_PLT_014()
    {
        var act = () => TipoEtapa.ObtenerTipoEtapa(99, new Dictionary<long, ParametroDetalle>());

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_PLT_014");
    }

    [Fact]
    public void ObtenerTipoAprobador_Debe_Resolver_Firmante_Y_Visador()
    {
        var dict = new Dictionary<long, ParametroDetalle>
        {
            [10] = new(10, "TI_APROBADOR_FIRMANTE", "F"),
            [11] = new(11, "TI_APROBADOR_VISADOR", "V")
        };

        TipoAprobador.ObtenerTipoAprobador(10, dict).EsFirmante.Should().BeTrue();
        TipoAprobador.ObtenerTipoAprobador(11, dict).EsVisador.Should().BeTrue();
    }
}
