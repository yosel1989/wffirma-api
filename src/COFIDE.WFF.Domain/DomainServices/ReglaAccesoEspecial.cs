using COFIDE.WFF.Domain.Common.Enums;

namespace COFIDE.WFF.Domain.DomainServices;

public static class ReglaAccesoEspecial
{
    public static bool EvaluarRolAccesoEspecial(
        string? codigoRol)
    {
        if (RolUsuarioCodigo.Desde(codigoRol) != RolUsuario.Coordinador)
            return false;

        return true;
    }
}
