namespace COFIDE.WFF.Api.Constants;
/// <summary>
/// Códigos estándar de respuesta alineados a grupos HTTP (2xx, 4xx, 5xx).
/// Úsados en ResponseDto.Code para representar errores o éxitos de manera uniforme.
/// </summary>
public static class ResponseCodes
{
    // 2xx - ÉXITOS
    public const string Success = "20000";          // Operación exitosa general

    public const string Created = "20100";          // Recurso creado exitosamente
    public const string Updated = "20001";          // Recurso actualizado
    public const string Deleted = "20002";          // Eliminación exitosa
    public const string NoContent = "20400";        // Sin contenido

    // 4xx - ERRORES DEL CLIENTE
    public const string ValidationError = "40001";  // Validación de campos fallida

    public const string BadRequest = "40000";       // Solicitud inválida
    public const string RouteMismatch = "40002";    // Ids de ruta no coinciden
    public const string Conflict = "40900";         // Conflicto (duplicado u otro)
    public const string NotFound = "40400";         // No encontrado
    public const string AlreadyDeleted = "41000";   // Ya eliminado
    public const string Unauthorized = "40100";     // No autenticado
    public const string Forbidden = "40300";        // No autorizado

    // 5xx - ERRORES DEL SERVIDOR
    public const string InternalError = "50000";    // Error inesperado

    public const string DependencyError = "50200";  // Error de servicio externo
    public const string Timeout = "50400";          // Timeout de operación

    // ===============================
    // === MÓDULOS ESPECÍFICOS    ====
    // ===============================

    // --- CONFIGURACIÓN / SISTEMA ---
    public const string Config_MissingKey = "CONFIG_40001";


    public const string OperacionExitosa = "Operacion exitosa";
    public const string RegistrosNoEncontrados = "No se encontraron registros";
}
