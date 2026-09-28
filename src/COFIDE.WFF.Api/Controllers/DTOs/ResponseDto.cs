using COFIDE.WFF.Api.Constants;
using COFIDE.WFF.Application.Common.Pageds;
using System.Text.Json.Serialization;

namespace COFIDE.WFF.Api.Controllers.DTOs;

/// <summary>
/// Representa una respuesta estándar de la API, con soporte para datos, errores, correlación y códigos semánticos.
/// </summary>
/// <typeparam name="T">Tipo del dato contenido en la respuesta.</typeparam>
public class ResponseDto<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Código asociado al resultado de la operación, alineado a <see cref="ResponseCodes"/>.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo sobre el resultado de la operación.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Objeto de datos resultante de la operación, si existe.
    /// No se incluye en la respuesta si su valor es nulo.
    /// </summary>
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; init; }


    /// <summary>
    /// Lista de errores ocurridos durante la operación, comúnmente utilizada en errores de validación.
    /// No se incluye en la respuesta si su valor es nulo.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ErrorDto>? Errors { get; init; }

    /// <summary>
    /// Identificador de correlación para rastrear solicitudes en logs o servicios distribuidos.
    /// No se incluye en la respuesta si su valor es nulo.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Constructor general (privado para asegurar uso controlado desde los métodos de fábrica).
    /// </summary>
    [JsonConstructor]
    public ResponseDto(
        bool success,
        T? data = default,
        string? message = null,
        List<ErrorDto>? errors = null,
        string? correlationId = null,
        string? code = null)
    {
        Success = success;
        Data = data;
        Message = message ?? string.Empty;
        Errors = errors;
        CorrelationId = correlationId;
        Code = code ?? (success ? ResponseCodes.Success : ResponseCodes.BadRequest);
    }

    /// <summary>
    /// Crea una respuesta exitosa, con soporte para datos, mensaje y código.
    /// </summary>
    public static ResponseDto<T> Ok(
        T? data,
        string? message = AppMessages.OperationSuccessful,
        string? correlationId = null,
        string? code = ResponseCodes.Success)
        => new(true, data, message, null, correlationId, code);

    /// <summary>
    /// Crea una respuesta de error con mensaje, errores opcionales, correlación y código.
    /// </summary>
    public static ResponseDto<T> Fail(
        string message = AppMessages.OperationFailed,
        List<ErrorDto>? errors = null,
        string? correlationId = null,
        string? code = ResponseCodes.BadRequest)
        => new(false, default, message, errors, correlationId, code);

    /// <summary>
    /// Crea una respuesta de error usando código y mensaje directo.
    /// </summary>
    public static ResponseDto<T> Fail(string code, string message)
    {
        var errors = new List<ErrorDto>
    {
        new ErrorDto(code, message, string.Empty)
    };
        return new ResponseDto<T>(false, default, message, errors, null, code);
    }

    /// <summary>
    /// Crea una respuesta de validación fallida, con lista de errores y código estándar.
    /// </summary>
    public static ResponseDto<T> FromValidation(
        string message,
        IEnumerable<ErrorDto> validationErrors,
        string? correlationId = null)
        => new(false, default, message, validationErrors.ToList(), correlationId, ResponseCodes.ValidationError);

    /// <summary>
    /// Agrega o reemplaza el ID de correlación en una respuesta ya creada.
    /// </summary>
    public ResponseDto<T> WithCorrelationId(string correlationId)
        => new(Success, Data, Message, Errors, correlationId, Code);

    /// <summary>
    /// Agrega o reemplaza el código en una respuesta ya creada.
    /// </summary>
    public ResponseDto<T> WithCode(string code)
        => new(Success, Data, Message, Errors, CorrelationId, code);

}
