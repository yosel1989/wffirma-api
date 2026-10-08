using COFIDE.WFF.Domain.Common.Enums;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Common;

public class RolUsuarioCodigoTest
{
    [Theory]
    [InlineData("ADMIN", RolUsuario.Administrador)]
    [InlineData("COORD", RolUsuario.Coordinador)]
    [InlineData("SOLAPR", RolUsuario.SolicitanteAprobador)]
    [InlineData(" coord ", RolUsuario.Coordinador)]
    public void Desde_Debe_Reconocer_Los_Codigos_Del_SUC(string codigo, RolUsuario esperado)
    {
        RolUsuarioCodigo.Desde(codigo).Should().Be(esperado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("OTRO")]
    public void Desde_Debe_Devolver_Null_Si_El_Codigo_No_Es_Un_Rol(string? codigo)
    {
        RolUsuarioCodigo.Desde(codigo).Should().BeNull();
    }
}
