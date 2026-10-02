using System.Text;
using Xunit.Sdk;

namespace COFIDE.WFF.Architecture.Tests.Support;

/// <summary>
/// Verificación con "trinquete": la prueba falla si aparecen violaciones nuevas
/// (no registradas en <see cref="LineaBase"/>) o si una violación registrada ya se corrigió
/// (para que la línea base solo pueda reducirse).
/// </summary>
public static class ArchitectureAssert
{
    public static void SinViolaciones(string regla, IEnumerable<string> violaciones, string recomendacion)
    {
        var actuales = violaciones
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToList();

        var permitidas = LineaBase.Para(regla);
        var nuevas = actuales.Where(v => !permitidas.Contains(v)).ToList();
        var corregidas = permitidas.Where(p => !actuales.Contains(p, StringComparer.Ordinal))
            .OrderBy(p => p, StringComparer.Ordinal).ToList();

        if (nuevas.Count == 0 && corregidas.Count == 0)
            return;

        var sb = new StringBuilder();
        sb.AppendLine($"Regla [{regla}]");
        sb.AppendLine($"Recomendación: {recomendacion}");

        if (nuevas.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"{nuevas.Count} violación(es) no registradas en la línea base:");
            foreach (var v in nuevas)
                sb.AppendLine($"  - {v}");
        }

        if (corregidas.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"{corregidas.Count} violación(es) de la línea base ya fueron corregidas. " +
                          "Elimínelas de LineaBase.cs para que no vuelvan a aparecer:");
            foreach (var v in corregidas)
                sb.AppendLine($"  - {v}");
        }

        if (nuevas.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Si la deuda es conocida y se corregirá más adelante, puede registrarla en LineaBase.cs:");
            sb.AppendLine($"[\"{regla}\"] = new(StringComparer.Ordinal)");
            sb.AppendLine("{");
            foreach (var v in actuales)
            {
                var escapado = v.Replace("\\", "\\\\").Replace("\"", "\\\"");
                sb.Append("    \"").Append(escapado).AppendLine("\",");
            }
            sb.AppendLine("},");
        }

        throw new XunitException(sb.ToString());
    }
}
