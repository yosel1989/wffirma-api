namespace COFIDE.WFF.Application.Common.Errors;

public static class ApplicationErrors
{
    public static readonly ApplicationError UserNotAuthenticated = new("AUTH_REQUIRED", "El usuario no está autenticado.");
    public static readonly ApplicationError AccessDenied = new("FORBIDDEN", "No tiene permisos para ingresar al Sistema.");
    public static readonly ApplicationError Forbidden = new("FORBIDDEN", "No tiene permisos para ejecutar esta acción.");
    public static readonly ApplicationError ValidationFailed = new("VALIDATION_FAILED", "La solicitud contiene errores de validación.");
    public static readonly ApplicationError Conflict = new("APPLICATION_CONFLICT", "La operación no puede completarse debido a un conflicto.");

    public static readonly ApplicationError SolicitudInvalid = new("APP_SOL_001", "La solicitud es inválida.");
    public static readonly ApplicationError TipoEtapaInvalid = new("APP_SOL_002", "Tipo de etapa inválida.");
    public static readonly ApplicationError DocumentoVacio = new("APP_SOL_003", "El documento se encuentra vacío.");
    public static readonly ApplicationError SustentoVacio = new("APP_SOL_004", "El documento de sustento se encuentra vacío.");
    public static readonly ApplicationError PlantillaInvalid = new("APP_SOL_005", "Plantilla inválida.");
    public static readonly ApplicationError DetalleFirmaInvalid = new("APP_SOL_006", "Detalle de Firma inválida.");
    public static readonly ApplicationError ConfiguracionInvalid = new("APP_SOL_011", "La configuración de flujo es inválida.");

    public static readonly ApplicationError FirmaDocumentoInvalid = new("APP_SOL_007", "Firma Documento: {0}.");
    public static readonly ApplicationError DocumentoInvalid = new("APP_SOL_008", "Documento inválido.");
    public static readonly ApplicationError ArchivoNotFound = new("APP_SOL_009", "No se logró obtener el documento en su ruta de origen.");
    

    public static readonly ApplicationError EnlaceNotFound = new("APP_SOL_010", "No se encontró enlace.");
    public static readonly ApplicationError DocumentoBloqueadoNotFound = new("APP_SOL_012", "No se encontró documento bloqueado.");
    public static readonly ApplicationError SolicitudOrigenNotFound = new("APP_SOL_013", "No se encontró la solicitud origen.");
    public static readonly ApplicationError FirmaArchivoNotFound = new("APP_SOL_014", "Firma Documento: El documento no se encuentra disponible");

    public static readonly ApplicationError DocumentosNotFound = new("APP_SOL_015", "No se encontraron los documentos.");
    public static readonly ApplicationError DocumentoNotFound = new("APP_SOL_015", "No se encontró el documento.");
    public static readonly ApplicationError DocumentoTagNotFound = new("APP_SOL_016", "No se encontró el tag.");

    public static readonly ApplicationError DocumentoFirmadoEmpty = new("APP_SOL_017", "El documento firmado está vacío.");
    public static readonly ApplicationError DocumentoEmpty = new("APP_SOL_017", "El documento firmado está vacío.");

    public static readonly ApplicationError UsuarioAreaAsignado = new("APP_SOL_018", "El usuario no tiene área asignada..");


    #region AccesoEspecial
    public static readonly ApplicationError AccesoEspecialNotFound = new("APP_SOL_019", "No se encontró el acceso especial.");
    public static readonly ApplicationError AccesoEspecialNotExists = new("APP_SOL_020", "El acceso especial ya no existe.");
    public static readonly ApplicationError AccesoEspecialUserAlreadyInArea = new("APP_SOL_021", "No se puede registrar un acceso especial a un área a la que el usuario ya pertenece.");
    #endregion

    public static readonly ApplicationError UsuarioNotFound = new("APP_SOL_022", "No se encontró el usuario o está inactivo.");
    public static readonly ApplicationError SolicitudSinAcceso = new("APP_SOL_023", "No tiene permiso para acceder a esta solicitud.");
}
