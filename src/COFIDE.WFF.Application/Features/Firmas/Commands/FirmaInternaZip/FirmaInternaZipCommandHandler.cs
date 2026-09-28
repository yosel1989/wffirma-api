using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.In;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaZip;

public class FirmaInternaZipCommandHandler(
        IMapper mapper,
        IFirmaInternaZipUseCase usecase
    ) : BaseCommandHandler<FirmaInternaZipCommand, FirmaInternaResponseDto?>(mapper)
{
    private readonly IFirmaInternaZipUseCase _usecase = usecase;

    public override async Task<FirmaInternaResponseDto?> Handle(FirmaInternaZipCommand request, CancellationToken cancellationToken)
    {
        return await _usecase.Firmar(request, cancellationToken);
    }
}
