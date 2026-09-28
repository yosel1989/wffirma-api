namespace COFIDE.WFF.Application.Features.Firmas.DTOs;

public sealed record InformacionPaginaPdfDto(
    decimal Ancho,
    decimal Alto,
    bool EsHorizontal
);