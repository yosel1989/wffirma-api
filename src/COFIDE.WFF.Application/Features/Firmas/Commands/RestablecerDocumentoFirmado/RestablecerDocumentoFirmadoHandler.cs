using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Firmas.Ports.In;

namespace COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;

public class RestablecerDocumentoFirmadoHandler(
        IMapper mapper,
        IRestablecerDocumentoFirmadoUseCase usecase
    ) : BaseCommandHandler<RestablecerDocumentoFirmadoCommand, string?>(mapper)
{
    private readonly IRestablecerDocumentoFirmadoUseCase _usecase = usecase;
    public override async Task<string?> Handle(RestablecerDocumentoFirmadoCommand request,CancellationToken cancellationToken)
    {
        return await _usecase.RestablecerFirma(request,cancellationToken);
    }
}

