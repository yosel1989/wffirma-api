using AutoMapper;
using COFIDE.WFF.Application.Common.Enums;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Publico.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Publico.DescargaDocumento;

public sealed class DescargaDocumentoQueryHandler(
        IMapper mapper,
        IEnlaceDescargaTemporalRepository enlaceDescargaTemporalRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ISolicitudRepository solicitudRepository
    ) : BaseQueryHandler<DescargaDocumentoQuery, DescargaDocumentoResponseDto>(mapper)
{
    private readonly IEnlaceDescargaTemporalRepository _enlaceDescargaTemporalRepository = enlaceDescargaTemporalRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;

    public override async Task<DescargaDocumentoResponseDto> Handle(DescargaDocumentoQuery request, CancellationToken cancellationToken)
    {
        var enlace = await _enlaceDescargaTemporalRepository.ObtenerPorTokenAsync(request.token, cancellationToken)
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.EnlaceNotFound);

        enlace.ValidarDisponible();

        var archivos = await _solicitudRepository.ObtenerDocumentosFirmadosPorSolicitudAsync(enlace.IdSolicitud, cancellationToken);

        enlace.RegistrarDescarga();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var archivosConRuta = new List<ArchivoDescargaDto>();

        foreach (var archivo in archivos)
        {
            var rutaCompleta = await _fileStorageService
                .GetRouteAsync(archivo.RutaFisica, cancellationToken);

            archivosConRuta.Add(new ArchivoDescargaDto(
                archivo.NombreArchivo,
                rutaCompleta));
        }

        if (archivosConRuta.Count == 1)
        {
            var archivo = archivosConRuta[0];

            return new DescargaDocumentoResponseDto(
                TipoDescarga.ArchivoUnico,
                archivo.NombreArchivo,
                "application/pdf",
                archivosConRuta);
        }

        var solicitud = await _solicitudRepository.ObtenerParaNotificaciones(enlace.IdSolicitud, cancellationToken)
                            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.EnlaceNotFound);

        return new DescargaDocumentoResponseDto(
                                    TipoDescarga.Zip,
                                    $"{solicitud.CodigoSolicitud}.zip",
                                    "application/zip",
                                    archivosConRuta);
    }
}