namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;
public sealed record ValidarDocumentoFirmadoResponseDto
{
    public string NombreDocumento { get; init; } = string.Empty;
    public bool TieneFirmaDigital { get; init; }
}