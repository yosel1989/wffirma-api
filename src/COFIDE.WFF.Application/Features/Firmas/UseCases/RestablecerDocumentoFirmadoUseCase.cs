using COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
using COFIDE.WFF.Application.Features.Firmas.Ports.In;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;

namespace COFIDE.WFF.Application.Features.Firmas.UseCases;

public class RestablecerDocumentoFirmadoUseCase(
        IDetalleFirmaRepository detalleFirmaRepository,
        IFileStorageService fileStorageService
    ) : IRestablecerDocumentoFirmadoUseCase
{

    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    public async Task<string?> RestablecerFirma(RestablecerDocumentoFirmadoCommand request, CancellationToken cancellationToken)
    {
        var detallesEnFirma = await _detalleFirmaRepository
            .ObtenerDocumentosEnEstadoFirmadoAsync(request.IdDetalleFirma, cancellationToken);

        if (detallesEnFirma.Count == 0)
            return "";

        foreach (var detallefirma in detallesEnFirma)
        {
            await _fileStorageService.DeleteFileAsync(detallefirma.ObtenerRutasArchivosFirmados(), cancellationToken);
            detallefirma.RestablecerFirma();
        }

        await _detalleFirmaRepository.UpdateRangeAsync(detallesEnFirma, cancellationToken);

        return "";
    }
}

