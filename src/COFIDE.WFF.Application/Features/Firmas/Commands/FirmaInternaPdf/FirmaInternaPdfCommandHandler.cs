using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Firmas.Ports.In;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaPdf;

public class FirmaInternaPdfCommandHandler(
        IMapper mapper,
        IFirmaInternaPdfUseCase usecase
    ) : BaseCommandHandler<FirmaInternaPdfCommand, FirmaInternaResponseDto?>(mapper)
{
    private readonly IFirmaInternaPdfUseCase _usecase = usecase;

    public override async Task<FirmaInternaResponseDto?> Handle(FirmaInternaPdfCommand request, CancellationToken cancellationToken)
    {
        return await _usecase.Firmar(request, cancellationToken);
    }
}
