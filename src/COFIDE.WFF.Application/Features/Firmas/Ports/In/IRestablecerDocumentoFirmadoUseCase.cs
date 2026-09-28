using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;

namespace COFIDE.WFF.Application.Features.Firmas.Ports.In
{
    public interface IRestablecerDocumentoFirmadoUseCase
    {
        Task<string?> RestablecerFirma(RestablecerDocumentoFirmadoCommand request, CancellationToken cancellationToken);
    }
}
