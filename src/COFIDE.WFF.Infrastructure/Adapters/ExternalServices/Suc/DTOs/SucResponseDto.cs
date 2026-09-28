namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Suc.DTOs;

public class SucResponseDto<T>
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public List<object>? Validations { get; set; }
    public T? Data { get; set; }
}
