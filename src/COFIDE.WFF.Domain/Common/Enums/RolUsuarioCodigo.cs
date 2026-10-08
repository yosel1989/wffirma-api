namespace COFIDE.WFF.Domain.Common.Enums;

/// <summary>
/// Códigos con los que el SUC identifica cada rol. Es el único lugar donde se escriben;
/// el resto del sistema trabaja con <see cref="RolUsuario"/>.
/// </summary>
public static class RolUsuarioCodigo
{
    public const string Administrador = "ADMIN";
    public const string Coordinador = "COORD";
    public const string SolicitanteAprobador = "SOLAPR";

    /// <summary>Convierte el código del SUC al rol. Devuelve null si el código no corresponde a ningún rol.</summary>
    public static RolUsuario? Desde(string? codigo) => codigo?.Trim().ToUpperInvariant() switch
    {
        Administrador => RolUsuario.Administrador,
        Coordinador => RolUsuario.Coordinador,
        SolicitanteAprobador => RolUsuario.SolicitanteAprobador,
        _ => null
    };
}
