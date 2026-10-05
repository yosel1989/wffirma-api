using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Support;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Solicitudes.ValueObjects;

public sealed record CorreoCase(string Caso, string Entrada, string Input, string? Esperado, string? CodigoError)
{
    public override string ToString() => Caso;
}

public class CorreoElectronicoTest
{
    public static IEnumerable<object[]> Casos =>
    JsonResource.Cases<CorreoCase>("Solicitudes/correo-electronico.json");


    [Theory]
    [MemberData(nameof(Casos))]
    public void Crear_Debe_Validar_El_Formato_Y_Normalizar_El_Valor(CorreoCase caso)
    {
        var act = () => CorreoElectronico.Crear(caso.Entrada);
        if (caso.CodigoError is not null)
        {
            act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(caso.CodigoError);
            return;
        }
        act().Value.Should().Be(caso.Esperado);
    }

    [Fact]
    public void Dos_Correos_Con_Distinta_Capitalizacion_Deben_Ser_Iguales()
    {
        var a = CorreoElectronico.Crear("Ana@cofide.com.pe"); var b = CorreoElectronico.Crear("ana@COFIDE.com.pe");
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }


}




