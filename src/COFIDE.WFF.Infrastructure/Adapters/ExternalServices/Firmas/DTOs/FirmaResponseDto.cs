namespace COFIDE.WFF.Infrastructure.Adapters.ExternalServices.Firmas.DTOs;
public class FirmaResponseDto<T>
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string? CorrelationId { get; set; }
}