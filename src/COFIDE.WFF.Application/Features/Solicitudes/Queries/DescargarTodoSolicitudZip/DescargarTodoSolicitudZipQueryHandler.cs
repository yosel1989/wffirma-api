using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarTodoSolicitudZip;

public class DescargarTodoSolicitudZipQueryHandler(
        IMapper mapper,
        ISolicitudDescargaZipService solicitudDescargaZipService
    ) : BaseQueryHandler<DescargarTodoSolicitudZipQuery, DescargarTodoSolicitudZipDto>(mapper)
{
    private readonly ISolicitudDescargaZipService _solicitudDescargaZipService = solicitudDescargaZipService;

    public override async Task<DescargarTodoSolicitudZipDto> Handle(
        DescargarTodoSolicitudZipQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _solicitudDescargaZipService.DescargarTodoZipAsync(
            request.Id,
            cancellationToken);


        return new DescargarTodoSolicitudZipDto
        {
            Archivo = result.Archivo,
            NombreArchivo = result.NombreZip,
            ContentType = "application/zip"
        };
    }
}