using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
namespace COFIDE.WFF.Application.Features.Firmas.Ports.In;

public interface IFirmaInternaPdfUseCase
{
    Task<FirmaInternaResponseDto?> Firmar(FirmaInternaPdfCommand request, CancellationToken cancellationToken);
}
