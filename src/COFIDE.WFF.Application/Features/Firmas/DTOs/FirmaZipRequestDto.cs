namespace COFIDE.WFF.Application.Features.Firmas.DTOs;
public class FirmaZipRequestDto
{
    public string FileName { get; set; } = default!;
    public Stream Content { get; set; } = default!;
}