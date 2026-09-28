using COFIDE.WFF.Domain.Common.Errors;
using COFIDE.WFF.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace COFIDE.WFF.Domain.ValueObjects;

public sealed record CorreoElectronico
{
    private const string PatronCorreo =
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public string Value { get; }

    private CorreoElectronico(string value)
    {
        Value = value;
    }

    public static CorreoElectronico Crear(string correo)
    {
        correo = correo.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(correo, PatronCorreo))
            throw new DomainException(
                new DomainError("CORREO_INVALIDO", $"El correo '{correo}' no tiene un formato válido."));

        return new CorreoElectronico(correo);
    }

    public override string ToString() => Value;
}