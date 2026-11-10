using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Common.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarSolicitudSustento;

public sealed class DescargarSolicitudSustentoHandler(
        IMapper mapper,
        IFileStorageService fileStorageService,
        ISolicitudRepository solicitudRepository,
        ISolicitudQueryRepository solicitudQueryRepository,
        IAccesoEspecialRepository accesoEspecialRepository,
        ICurrentUserService currentUser
    ) : BaseQueryHandler<DescargarSolicitudSustentoQuery, DescargarSolicitudSustentoDto>(mapper)
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;
    private readonly ICurrentUserService _currentUser = currentUser;

    public override async Task<DescargarSolicitudSustentoDto> Handle(
        DescargarSolicitudSustentoQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal.Value;

        var solicitudSustento = await _solicitudRepository
            .ObtenerSolicitudSustentoPorId(
                request.IdSolicitudSustento,
                cancellationToken,
                tracking: false);

        if (solicitudSustento is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SustentoVacio);

        // CA05 – Solo accesos especiales activos (GetAreasByUserIdAsync filtra EstadoCodigo.Activo).
        var idsAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(idUsuario);

        // CA08 – Abrir el detalle / descargar documentos: puede ver la solicitud por su rol
        // OR por un acceso especial activo (misma regla que el listado).
        var puedeVer = await _solicitudQueryRepository.PuedeVerSolicitudAsync(
            solicitudSustento.IdSolicitud,
            idUsuario,
            _currentUser.UserAreaId,
            _currentUser.CodigoRolUsuario,
            idsAreasAccesoEspecial,
            cancellationToken);

        if (!puedeVer)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudSinAcceso);

        var ruta = _fileStorageService.CombinarRuta(
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