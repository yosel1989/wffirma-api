using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudDocumentoZip;

public class DescargarSolicitudDocumentoZipQueryHandler(
        IMapper mapper,
        ISolicitudDescargaZipService solicitudDescargaZipService
    ) : BaseQueryHandler<DescargarSolicitudDocumentoZipQuery, DescargarSolicitudDocumentoZipDto>(mapper)
{
    private readonly ISolicitudDescargaZipService _solicitudDescargaZipService = solicitudDescargaZipService;

    public override async Task<DescargarSolicitudDocumentoZipDto> Handle(DescargarSolicitudDocumentoZipQuery request, CancellationToken cancellationToken)
    {
        var result = await _solicitudDescargaZipService.DescargarDocumentosZipAsync(
            request.Id,
            request.Documentos,
            cancellationToken);

        return new DescargarSolicitudDocumentoZipDto
        {
            Archivo = result.Archivo,
            NombreArchivo = result.NombreZip,
            ContentType = "application/zip"
        };
    }

}

