namespace COFIDE.WFF.Api.Controllers.DTOs;
/// <summary>
/// Representa un error simple con código y mensaje.
/// </summary>
public class ErrorDto
{
    public string Code { get; init; }
    public string Message { get; init; }
    public string Field { get; init; }

    public ErrorDto(string code, string message, string field)
    {
        Code = code;
        Message = message;
        Field = field;
    }
}
