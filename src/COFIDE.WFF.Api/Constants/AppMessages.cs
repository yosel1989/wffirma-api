namespace COFIDE.WFF.Api.Constants;
public static class AppMessages
{
    // General
    public const string CreatedSuccessfully = "El recurso se ha creado correctamente.";

    public const string UpdatedSuccessfully = "El recurso se ha actualizado correctamente.";
    public const string DeletedSuccessfully = "El recurso se ha eliminado correctamente.";
    public const string RetrievedSuccessfully = "Datos obtenidos correctamente.";
    public const string OperationSuccessful = "Operación realizada con éxito.";
    public const string OperationFailed = "La operación no se pudo completar.";
    public const string GeneralError = "Ha ocurrido un error inesperado. Intente nuevamente o contacte a soporte..";
    public const string NoChangesDetected = "No se detectaron cambios para actualizar.";
    public const string ActionNotAllowed = "Esta acción no está permitida.";

    // Request / Response Errors
    public const string NotFound = "El recurso solicitado no existe.";

    public const string BadRequest = "La solicitud es inválida o contiene datos incorrectos.";
    public const string InvalidRequest = "La solicitud no cumple con los parámetros requeridos.";
    public const string RouteIdMismatch = "El ID proporcionado en la ruta no coincide con el del cuerpo de la solicitud.";
    public const string MissingRequiredFields = "Faltan campos obligatorios en la solicitud.";
    public const string MalformedJson = "El formato del cuerpo de la solicitud es inválido.";

    // Auth / Security
    public const string UnauthorizedAccess = "Acceso no autorizado. Inicia sesión para continuar.";

    public const string ForbiddenAccess = "Acceso prohibido. No tienes los permisos necesarios.";
    public const string InvalidCredentials = "Credenciales incorrectas. Verifica tu usuario y contraseña.";
    public const string AccessDenied = "No tienes permisos suficientes para realizar esta acción.";
    public const string SessionExpired = "La sesión ha expirado. Por favor, vuelve a iniciar sesión.";
    public const string TokenInvalid = "El token proporcionado no es válido.";
    public const string AccountLocked = "La cuenta se encuentra bloqueada. Contacta al administrador.";

    // Validation
    public const string ValidationFailed = "Uno o más campos no pasaron la validación.";

    public const string FieldValidationError = "Error de validación en uno o más campos.";
    public const string DuplicateRecord = "Ya existe un registro con los mismos datos.";

    // Pagination
    public const string PagedDataSuccess = "Datos paginados obtenidos exitosamente.";

    public const string NoResultsFound = "No se encontraron resultados.";

    // Domain-Specific (Custom)
    public const string UserNotFound = "El usuario especificado no existe.";

    public const string EmailAlreadyInUse = "El correo electrónico ingresado ya está registrado.";
    public const string UsernameAlreadyTaken = "El nombre de usuario ya está en uso.";
    public const string AccountNotVerified = "La cuenta no ha sido verificada aún.";

    // Entidad en general
    public const string EntityNotFound = "{0} con ID {1} no fue encontrado(a).";

    public const string EntityCreated = "{0} se creó exitosamente.";
    public const string EntityUpdated = "{0} se actualizó correctamente.";
    public const string EntityDeleted = "{0} con ID {1} fué eliminado correctamente.";
    public const string EntityInUse = "{0} está en uso y no puede eliminarse.";
    public const string EntityAlreadyExists = "{0} ya existe en el sistema.";
    public const string EntityAlreadyDeleted = "{0} ya fue eliminado anteriormente.";
    public const string PagedEntityDataRetrieved = "Se obtuvo correctamente la lista de {0}.";
    public const string NoEntityResultsFound = "No se encontraron resultados de {0}.";
    public const string EntityUpdateInUse = "{0} está en uso y no puede actualizarse.";

}
