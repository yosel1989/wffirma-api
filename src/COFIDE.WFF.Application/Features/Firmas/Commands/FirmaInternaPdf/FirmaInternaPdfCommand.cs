using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;

public sealed record FirmaInternaPdfCommand
(
    long IdSolicitud,
    long IdSolicitudDocumento
) : CreateCommand<FirmaInternaResponseDto?>;