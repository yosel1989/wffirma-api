
using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustentoZip;

public class DescargarSolicitudSustentoZipQueryHandler(
        IMapper mapper,
        ISolicitudDescargaZipService solicitudDescargaZipService,
        ISolicitudConsultaAuthorizationService solicitudConsultaAuthorizationService
    ) : BaseQueryHandler<DescargarSolicitudSustentoZipQuery, DescargarSolicitudSustentoZipDto>(mapper)
{
    private readonly ISolicitudDescargaZipService _solicitudDescargaZipService = solicitudDescargaZipService;
    private readonly ISolicitudConsultaAuthorizationService _solicitudConsultaAuthorizationService = solicitudConsultaAuthorizationService;

    public override async Task<DescargarSolicitudSustentoZipDto> Handle(DescargarSolicitudSustentoZipQuery request, CancellationToken cancellationToken)
    {
        await _solicitudConsultaAuthorizationService.AutorizarPorIdAsync(request.Id, cancellationToken);

        var result = await _solicitudDescargaZipService.DescargarSustentosZipAsync(
            request.Id,
            request.Sustentos,
            cancellationToken);

        return new DescargarSolicitudSustentoZipDto
        {
            Archivo = result.Archivo,
            NombreArchivo = result.NombreZip,
            ContentType = "application/zip"
        };
    }
}