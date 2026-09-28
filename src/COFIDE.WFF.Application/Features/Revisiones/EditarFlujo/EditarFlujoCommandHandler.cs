using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Services;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Services;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;

public sealed class EditarFlujoCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        ISolicitudRepository solicitudRepository,
        ISolicitudPlantillaRepository solicitudPlantillaRepository,
        IPlantillaRepository plantillaRepository,
        IPlantillaCreacionService plantillaCreacionService,
        IWorkflowDomainService workflowDomainService,
        IParametroRepository parametroRepository,
        ISolicitudFlujoDiffService solicitudFlujoDiffService,
        ISolicitudEventoFactory solicitudEventoFactory,
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository
    ) : BaseCommandHandler<EditarFlujoCommand, EditarFlujoResponseDto>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly ISolicitudPlantillaRepository _solicitudPlantillaRepository = solicitudPlantillaRepository;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IPlantillaCreacionService _plantillaCreacionService = plantillaCreacionService;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly ISolicitudFlujoDiffService _solicitudFlujoDiffService = solicitudFlujoDiffService;
    private readonly ISolicitudEventoFactory _solicitudEventoFactory = solicitudEventoFactory;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;

    public override async Task<EditarFlujoResponseDto> Handle(EditarFlujoCommand request, CancellationToken cancellationToken)
    {
        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;
        bool redireccionar = false;

        // Obtener la Solicitud por ID
        var solicitud = await _solicitudRepository.ObtenerAgregadoSolicitudAsync(request.Id, cancellationToken, true)
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.SolicitudInvalid);

        if (solicitud.SolicitudPlantilla.IdPlantilla != request.IdPlantilla)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        // Obtener la Plantilla por Id
        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(request.IdPlantilla, cancellationToken, true)
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        var tipoAprobadorDict = (await _parametroRepository
                .ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", cancellationToken))
                .ToDictionary(
                    x => x.Id,
                    x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
                );

        // Seteamos el Usuarioasignado al firmante aún vigente
        _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, tipoAprobadorDict);

        // Obtener el último id de Firmante Flujo
        long idSolicitudFirmante = await _solicitudRepository.ObtenerSiguienteIdSolFirmanteAsync(cancellationToken);


        // Creamos la plantilla cuando este es maestro

        var indexSolicitudFirmante = 0;
        var cantidadNuevosSolicitudFirmantes = request.Etapas!
                                    .SelectMany(e => e.Firmantes ?? [])
                                    .Count(f => f.Id == 0);

        var rangoSolicitudFirmantes = await _solicitudRepository.ObtenerBloqueSiguienteIdSolFirmanteAsync(cantidadNuevosSolicitudFirmantes, cancellationToken);
        Func<long> generarIdSolicitudFirmante = () => rangoSolicitudFirmantes[indexSolicitudFirmante++];

        if (plantilla.EsPlantillaMaestra)
        {
            var input = EditarFlujoMapping.MapearInputPlantilla(request, plantilla);
            var nuevaPlantilla = await _plantillaCreacionService.CrearPlantilla(input, usuarioActual, gerenciaUsuario, cancellationToken);
            await _plantillaRepository.AddAsync(nuevaPlantilla);
            solicitud.SolicitudPlantilla.ActualizarPlantilla(nuevaPlantilla.Id);
            _workflowDomainService.ActualizarSolicitudFirmanteMaestroFlujo(solicitud.SolicitudPlantilla, nuevaPlantilla, idSolicitudFirmante);
        }
        else
        {
            var tipoEtapaDict = (await _parametroRepository
               .ObtenerDetallePorCodigoParametro("TIPO_ETAPA", cancellationToken))
               .ToDictionary(
                   x => x.Id,
                   x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
               );

            var editPlantilla = EditarFlujoMapping.MapearEditPlantilla(request);

            var cantidadNuevasEtapas = request.Etapas!.Count(f => f.Id == 0);
            var cantidadNuevosFirmantes = request.Etapas!
                                            .SelectMany(e => e.Firmantes ?? [])
                                            .Count(f => f.Id == 0);
            var cantidadNuevosDetalleFirma = (cantidadNuevosFirmantes * (solicitud.Documentos.Count));

            var rangoEtapas = await _plantillaRepository.ObtenerBloqueSiguienteIdEtapaAsync(cantidadNuevasEtapas, cancellationToken);
            var rangoFirmantes = await _plantillaRepository.ObtenerBloqueSiguienteIdFirmanteAsync(cantidadNuevosFirmantes, cancellationToken);
            var rangoDetalleFirma = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdDetalleFirmaAsync(cantidadNuevosDetalleFirma, cancellationToken);

            var indexEtapa = 0; var indexFirmante = 0; var indexDetalleFirma = 0;
            Func<long> generarIdEtapa = () => rangoEtapas[indexEtapa++];
            Func<long> generarIdFirmante = () => rangoFirmantes[indexFirmante++];
            Func<long> generarIdDetalleFirma = () => rangoDetalleFirma[indexDetalleFirma++];

            // Validación previa
            _workflowDomainService.ValidacionFirmanteNoMaestroFlujo(solicitud.SolicitudPlantilla, plantilla, editPlantilla);

            // Obtenemos los cambios que han mapeado en el flujo
            var diff = _solicitudFlujoDiffService.CalcularDiff(plantilla, editPlantilla);

            // Editamos el flujo del agregado Plantilla => Etapas y FirmanteFlujo
            plantilla.EditarFlujo(editPlantilla, generarIdEtapa, generarIdFirmante, tipoEtapaDict, tipoAprobadorDict, usuarioActual);

            // Actualizar el idPlantillaEtapa para los eventos
            var etapaMap = plantilla.Etapas
                .Where(e => e.EsNuevo && e.TempId.HasValue)
                .ToDictionary(e => e.TempId!.Value, e => e.Id);

            ActualizarIdEtapasEnDiff(diff, etapaMap);

            var estados = solicitud.ObtenerEstadoEtapas();

            var ultimaEtapaBloqueada = estados
                .Where(e => e.Bloqueado)
                .Select(e => e.OrdenEtapa)
                .DefaultIfEmpty(0)
                .Max();

            var ordenesNuevos = new List<long>();
            ordenesNuevos.AddRange(diff.EtapasAgregadas.Select(x => x.Orden));
            ordenesNuevos.AddRange(diff.EtapasMovidas.Select(x => x.OrdenNuevo));
            ValidarReordenamiento(ordenesNuevos, ultimaEtapaBloqueada);
            _workflowDomainService.ActualizarSolicitudFirmanteNoMaestroFlujo(solicitud.SolicitudPlantilla, plantilla, generarIdSolicitudFirmante, generarIdDetalleFirma, null);

            _plantillaRepository.GuardarFlujoPlantillaAsync(plantilla);

            var eventos = _solicitudEventoFactory.CrearEventos(solicitud.Id, diff);
            solicitud.RegistrarEventos(eventos);
        }

        await _solicitudPlantillaRepository.UpdateAsync(solicitud.SolicitudPlantilla);

        redireccionar = solicitud.DebeRedirigir(usuarioActual);

        return new EditarFlujoResponseDto { Redireccionar = redireccionar };
    }

    private static void ActualizarIdEtapasEnDiff(
        SolicitudFlujoDiff diff,
        Dictionary<Guid, long> etapaMap)
    {
        for (int i = 0; i < diff.EtapasAgregadas.Count; i++)
        {
            var e = diff.EtapasAgregadas[i];
            if (etapaMap.TryGetValue(e.TempId, out var idReal))
                diff.EtapasAgregadas[i] = e with { IdEtapa = idReal };
        }

        for (int i = 0; i < diff.FirmantesAgregados.Count; i++)
        {
            var f = diff.FirmantesAgregados[i];
            if (f.TempIdEtapa != Guid.Empty &&
                etapaMap.TryGetValue(f.TempIdEtapa, out var idEtapaReal))
            {
                diff.FirmantesAgregados[i] = f with { IdEtapa = idEtapaReal };
            }
        }
    }

    private static void ValidarReordenamiento(IEnumerable<long> ordenesNuevos, long ultimaEtapaBloqueada)
    {
        if (ordenesNuevos.Any(o => o <= ultimaEtapaBloqueada))
            throw new DomainException(SolicitudErrors.CrearEtapaPrevia);
    }
}
