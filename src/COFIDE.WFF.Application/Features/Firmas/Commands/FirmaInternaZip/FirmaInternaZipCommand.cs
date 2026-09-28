using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;

public sealed record FirmaInternaZipCommand
(
    long IdSolicitud,
    List<long> Documentos
) : CreateCommand<FirmaInternaResponseDto?>;
