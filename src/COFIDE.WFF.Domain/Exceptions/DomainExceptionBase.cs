using COFIDE.WFF.Domain.Common.Errors;
namespace COFIDE.WFF.Domain.Exceptions;
/// <summary>
/// Excepción genérica para violaciones de reglas de negocio en el dominio.
/// Se lanza cuando una entidad o regla de dominio no cumple los invariantes definidos.
/// </summary>
public abstract class DomainExceptionBase : Exception
{
    public DomainError Error { get; }

    protected DomainExceptionBase(
        DomainError error,
        Exception? inner = null)
        : base(error.Description, inner)
    {
        Error = error;
    }
}
