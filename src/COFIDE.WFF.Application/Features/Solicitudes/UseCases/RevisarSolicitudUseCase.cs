using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.In;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Application.Common.Files;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Application.Features.Publico.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Factories;
using VO = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Application.Features.Solicitudes.UseCases;

public class RevisarSolicitudUseCase(
        ICurrentUserService currentUser,
        ISolicitudRepository solicitudRepository,
        IPlantillaRepository plantillaRepository,
        IWorkflowDomainService workflowDomainService,
        IParametroRepository parametroRepository,
        ISolicitudSustentoService solicitudSustentoService,
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository,
        IFileStorageService fileStorageService,
        IEnlaceDescargaTemporalRepository enlaceDescargaTemporalRepository,
        ITokenSeguroGenerator tokenSeguroGenerator
    ) : IRevisarSolicitudUseCase
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly ISolicitudSustentoService _solicitudSustentoService = solicitudSustentoService;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IEnlaceDescargaTemporalRepository _enlaceDescargaTemporalRepository = enlaceDescargaTemporalRepository;
    private readonly ITokenSeguroGenerator _tokenSeguroGenerator = tokenSeguroGenerator;

    public async Task<string?> Aprobar(AprobarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        long IsUsuarioAccion = _currentUser.UserIdLocal!.Value;
        string datosCompletos = _currentUser.NombreUsuario + " " + _currentUser.ApellidoPaterno + " " + _currentUser.ApellidoMaterno;

        var solicitud = await PrepararAprobacionRechazo(cmd.Id, cmd.Sustentos, cancellationToken);

        solicitud.AprobarFirmante(IsUsuarioAccion, datosCompletos);
        if (!string.IsNullOrWhiteSpace(cmd.Comentario))
        {
            long idComentario = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdComentarioAsync(cancellationToken);
            solicitud.AgregarComentario(idComentario, IsUsuarioAccion, cmd.Comentario);
        }

        // el enlace de descarga se crea dentro de la transacción de la aprobación
        // (antes lo creaba el handler del correo, después del commit y fuera de la transacción).
        if (solicitud.IdEstadoSolicitud == VO.EstadoSolicitud.Aprobado.Codigo
            && solicitud.Documentos.Any(d => d.Estado))
        {
            await CrearEnlaceDescargaAsync(solicitud.Id, cancellationToken);
        }

        _solicitudRepository.GuardarFlujoSolicitudAsync(solicitud);
        return solicitud.CodigoSolicitud;
    }

    private async Task CrearEnlaceDescargaAsync(long idSolicitud, CancellationToken cancellationToken)
    {
        var parametroVigencia = await _parametroRepository
            .ObtenerCodigoParametroDias(CodigoParametroVigenciaEnlace, cancellationToken);

        var vigenciaDias = int.TryParse(parametroVigencia.FirstOrDefault()?.Valor, out var dias) ? dias : 1;

        var enlace = EnlaceDescargaTemporal.Crear(
            _tokenSeguroGenerator.Generar(),
            idSolicitud,
            DateTime.Now.AddDays(vigenciaDias));

        await _enlaceDescargaTemporalRepository.AddAsync(enlace);
    }

    private const string CodigoParametroVigenciaEnlace = "TI_VIGENCIA";

    public async Task<string?> Rechazar(RechazarSolicitudCommand cmd, CancellationToken cancellationToken)
    {
        string areaUsuario = _currentUser.UserAreaNombre!;
        long IsUsuarioAccion = _currentUser.UserIdLocal!.Value;
        string datosCompletos = _currentUser.NombreUsuario + " " + _currentUser.ApellidoPaterno + " " + _currentUser.ApellidoMaterno;

        var solicitud = await PrepararAprobacionRechazo(cmd.Id, cmd.Sustentos, cancellationToken);
        var idPñantillaEtapaActual = solicitud.SolicitudPlantilla.ObtenerIdPlantillaEtapaActualPorUsuarioAsignado(IsUsuarioAccion);

        solicitud.RechazarSolicitud(IsUsuarioAccion, datosCompletos, cmd.MotivoRechazo, idPñantillaEtapaActual, areaUsuario);

        if (!string.IsNullOrWhiteSpace(cmd.Comentario))
        {
            long idComentario = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdComentarioAsync(cancellationToken);
            solicitud.AgregarComentario(idComentario, IsUsuarioAccion, cmd.Comentario);
        }

        foreach (var doc in solicitud.Documentos)
        {
            foreach (var item in doc.ObtenerRutasArchivosFirmados())
            {
                await _fileStorageService.DeleteFileAsync(item, cancellationToken);
            }
        }

        _solicitudRepository.GuardarFlujoSolicitudAsync(solicitud);
        return solicitud.CodigoSolicitud;
    }

    private async Task<Solicitud> PrepararAprobacionRechazo(long idSolicitud, List<ArchivoEntrada>? sustentos, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepository.ObtenerSolicitudParaAprobacion(idSolicitud, cancellationToken, true)
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        var pantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(solicitud.SolicitudPlantilla.IdPlantilla, cancellationToken)
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        var tipoAprobadorDict = (await _parametroRepository
               .ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", cancellationToken))
               .ToDictionary(
                   x => x.Id,
                   x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
               );

        solicitud = _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, pantilla, tipoAprobadorDict);
        var sustentosInput = await _solicitudSustentoService.CrearDocumentoSustentoInput(sustentos, solicitud.CodigoSolicitud, cancellationToken);

        var cantidadNuevosSustentos = sustentos?.Count ?? 0;
        var rangoSustentos = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdSustentoAsync(cantidadNuevosSustentos, cancellationToken);
        var indexSustento = 0;
        Func<long> generarIdSustento = () => rangoSustentos[indexSustento++];

        foreach (var nuevoSustento in sustentosInput)
        {
            var idSustento = generarIdSustento();
            solicitud.AgregarSustento(idSustento, nuevoSustento.NombreSustento, nuevoSustento.ExtensionSustento, nuevoSustento.RutaSustento, nuevoSustento.PesoSustento);
            var eventos = SolicitudEventoFactory.CrearEventoSustento(solicitud.Id, idSustento, _currentUser.UserIdLocal!.Value, _currentUser.UserGerenciaNombre!);
            solicitud.RegistrarEventos(eventos);
        }

        return solicitud;
    }
}
