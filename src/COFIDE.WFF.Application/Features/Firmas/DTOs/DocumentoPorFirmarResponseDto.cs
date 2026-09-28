namespace COFIDE.WFF.Application.Features.Firmas.DTOs;

public class DocumentoPorFirmarResponseDto
{
    public string CodigoSolicitud { get; init; } = default!;
    public bool InCodigo { get; init; }
}
