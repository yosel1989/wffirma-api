using COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
namespace COFIDE.WFF.Application.Features.Firmas.Ports.In;

public interface IFirmaInternaZipUseCase
{

    /// <summary>
    /// Esta opción solo es para: Firmantes --> Sólo firma TAGS | Visadores --> firma hasta 11 visadores con parámetros
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FirmaInternaResponseDto?> Firmar(FirmaInternaZipCommand request, CancellationToken cancellationToken);    
}
