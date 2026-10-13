using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;

namespace COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;

public class RestablecerDocumentoFirmadoHandler(
        IMapper mapper,
        IDetalleFirmaRepository detalleFirmaRepository,
        IFileStorageService fileStorageService
    ) : BaseCommandHandler<RestablecerDocumentoFirmadoCommand, string?>(mapper)
{

    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    public override async Task<string?> Handle(RestablecerDocumentoFirmadoCommand request, CancellationToken cancellationToken)
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

