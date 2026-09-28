using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustento;

public sealed class DescargarSolicitudSustentoHandler(
        IMapper mapper,
        IFileStorageService fileStorageService,
        ISolicitudRepository solicitudRepository
    ) : BaseQueryHandler<DescargarSolicitudSustentoQuery, DescargarSolicitudSustentoDto>(mapper)
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;

    public override async Task<DescargarSolicitudSustentoDto> Handle(
        DescargarSolicitudSustentoQuery request,
        CancellationToken cancellationToken)
    {
        var solicitudSustento = await _solicitudRepository
            .ObtenerSolicitudSustentoPorId(
                request.IdSolicitudSustento,
                cancellationToken,
                tracking: false);

        if (solicitudSustento is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SustentoVacio);

        var ruta = Path.Combine(
                solicitudSustento.RutaSustento,
                $"{solicitudSustento.NombreSustento}.{solicitudSustento.ExtensionSustento}");

        await using var stream = await _fileStorageService.GetFileAsync(ruta, cancellationToken);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var archivoBase64 = Convert.ToBase64String(memoryStream.ToArray());

        return new DescargarSolicitudSustentoDto
        {
            Nombre = solicitudSustento.NombreSustento,
            Extension = solicitudSustento.ExtensionSustento,
            ArchivoBase64 = archivoBase64
        };
    }
}
