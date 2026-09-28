using COFIDE.WFF.Application.Common.Errors;
namespace COFIDE.WFF.Infrastructure.Persistence;

public static class PersistenceErrors
{
    public static readonly ApplicationError ConcurrencyConflict =
        new("DB_CONCURRENCY_CONFLICT",
            "Se detectó un conflicto de concurrencia al guardar los cambios.");

    public static readonly ApplicationError SaveFailed =
        new("DB_SAVE_FAILED",
            "Ocurrió un error al guardar los cambios en la base de datos.");

    public static readonly ApplicationError NoActiveTransaction =
        new("DB_NO_ACTIVE_TRANSACTION",
            "No hay una transacción activa para confirmar.");
}
