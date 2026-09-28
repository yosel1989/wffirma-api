namespace COFIDE.WFF.Application.Common.Results;

public class Result<T>
{
    public bool Success { get; }
    public string Code { get; }
    public string Message { get; }
    public T? Data { get; }
    public List<string>? Errors { get; }

    private Result(bool success, T? data, string code, string message, List<string>? errors = null)
    {
        Success = success;
        Data = data;
        Code = code;
        Message = message;
        Errors = errors;
    }

    public static Result<T> Ok(T data, string message = "Operación exitosa")
        => new(true, data, "200", message);

    public static Result<T> Fail(string code, string message, List<string>? errors = null)
        => new(false, default, code, message, errors);
}
