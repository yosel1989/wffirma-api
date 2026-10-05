using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
using System.Text.Json;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudPlantillaService(
        IPlantillaRepository plantillaRepository,
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository,
        IParametroRepository parametroRepository,
        IFlujoDiffDomainService flujoDiffDomainService,
        IWorkflowDomainService workflowDomainService
    ) : ISolicitudPlantillaService
{
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly IFlujoDiffDomainService _flujoDiffDomainService = flujoDiffDomainService;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;

    public async Task<Plantilla?> CrearSolicitudPlantilla(long? idPlantillaOrigen, string? configuracion, long? idPlantillaRequest, long idUsuario, string gerenciaUsuario, CancellationToken cancellationToken)
    {
        var idPlantillaReq = idPlantillaRequest ?? 0;
        var flujo = ObtenerFlujoDesdeConfiguracion(configuracion);
        var plantilla = await ObtenerPlantillaExistente(idPlantillaReq, cancellationToken);

        if ((idPlantillaRequest is null or 0) || (idPlantillaOrigen == 0 && plantilla.EsPlantillaMaestra && flujo != null))
        {
            if (flujo is null)
                return null;

            return await CrearNuevaPlantillaNoMaestra(
                flujo,
                idUsuario,
                gerenciaUsuario,
                cancellationToken);
        }

        if (plantilla.EsPlantillaMaestra && idPlantillaOrigen == 0)
            return await CrearDesdePlantillaMaestra(plantilla, idUsuario, gerenciaUsuario, cancellationToken);

        if (idPlantillaOrigen != idPlantillaRequest)
        {
            if (flujo == null)
                flujo = ClonarPlantillaMaestra_a_FlujoInterno(plantilla);
            else
                flujo = ResetearIdsPlantillaMaestra(flujo);

            plantilla = await ObtenerPlantillaExistente(idPlantillaOrigen!.Value, cancellationToken);
        }


        await ActualizarPlantillaSiCorresponde(flujo, plantilla, idPlantillaRequest.Value, idUsuario, cancellationToken);

        return plantilla;
    }

    public async Task ActualizarFlujoDesdeSolicitud(Plantilla? plantilla, Solicitud solicitud, CancellationToken cancellationToken)
    {
        if (plantilla == null)
            return;

        // Obtenemos el diccionario de tipo de aprobadores
        var tipoAprobadorDict = await ObtenerDiccionarioTipoAprobador(cancellationToken);

        var cantidadNuevosFirmantes = plantilla.Etapas!.SelectMany(e => e.Firmantes!).Count();
        var cantidadNuevosDetalleFirma = (cantidadNuevosFirmantes * (solicitud.Documentos.Count));
        var rangoSolicitudFirmantes = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(cantidadNuevosFirmantes, cancellationToken);
        var rangoDetalleFirma = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdDetalleFirmaAsync(cantidadNuevosDetalleFirma, cancellationToken);
        var indexSolicitudFirmante = 0; var indexDetalleFirma = 0;
        Func<long> generarIdSolicitudFirmante = () => rangoSolicitudFirmantes[indexSolicitudFirmante++];
        Func<long> generarIdDetalleFirma = () => rangoDetalleFirma[indexDetalleFirma++];

        if (solicitud.SolicitudPlantilla == null)
        {
            long idSolicitudPlantilla = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdSolicitudPlantillaAsync(cancellationToken);
            // Asignamos la plantilla con la solicitud
            var solicitudPlantilla = SolicitudPlantilla.Crear(idSolicitudPlantilla, solicitud, plantilla);
            solicitud.AsignarPlantilla(solicitudPlantilla);
        }

        // Seteamos el Usuarioasignado al firmante aún vigente
        _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, tipoAprobadorDict);

        solicitud.GenerarDetallesFirmaPendientes(generarIdDetalleFirma);

        _workflowDomainService.ActualizarSolicitudFirmanteNoMaestroFlujo(solicitud.SolicitudPlantilla, plantilla, generarIdSolicitudFirmante, generarIdDetalleFirma, null);
    }

    public async Task CrearFlujoDesdeSolicitud(Plantilla? plantilla, Solicitud solicitud, CancellationToken cancellationToken)
    {
        if (plantilla == null)
            return;

        // Obtenemos los ID's
        long idSolicitudPlantilla = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdSolicitudPlantillaAsync(cancellationToken);

        var cantidadNuevosFirmantes = plantilla.Etapas!.SelectMany(e => e.Firmantes!).Count();
        var cantidadNuevosDetalleFirma = (cantidadNuevosFirmantes * (solicitud.Documentos.Count));
        var rangoSolicitudFirmantes = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(cantidadNuevosFirmantes, cancellationToken);
        var rangoDetalleFirma = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdDetalleFirmaAsync(cantidadNuevosDetalleFirma, cancellationToken);
        var indexSolicitudFirmante = 0; var indexDetalleFirma = 0;
        Func<long> generarIdSolicitudFirmante = () => rangoSolicitudFirmantes[indexSolicitudFirmante++];
        Func<long> generarIdDetalleFirma = () => rangoDetalleFirma[indexDetalleFirma++];

        // Asignamos la plantilla con la solicitud
        var solicitudPlantilla = SolicitudPlantilla.Crear(idSolicitudPlantilla, solicitud, plantilla);
        solicitud.AsignarPlantilla(solicitudPlantilla);

        // Creamos los firmantes en la solicitud en base a la plantilla base
        _workflowDomainService.AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, generarIdSolicitudFirmante);

        solicitud.GenerarDetallesFirmaPendientes(generarIdDetalleFirma);
    }

    #region Métodos privados

    private async Task<Plantilla> CrearNuevaPlantillaNoMaestra(CrearSolicitudPlantilla flujo, long idUsuario, string gerenciaUsuario, CancellationToken cancellationToken)
    {
        var tipoEtapaDict = await ObtenerDiccionarioTipoEtapa(cancellationToken);
        var tipoAprobadorDict = await ObtenerDiccionarioTipoAprobador(cancellationToken);
        long idPlantilla = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdPlantillaAsync(cancellationToken);
        long idEtapa = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdEtapaAsync(cancellationToken);
        long idFirmante = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdFirmanteAsync(cancellationToken);

        flujo.Id = idPlantilla;

        var requestMapper = MapearInputPlantilla(flujo);
        var plantilla = PlantillaFactory.CrearDesdeRequest(
             requestMapper,
             idPlantilla,
             idEtapa,
             idFirmante,
             idUsuario,
             gerenciaUsuario,
             tipoEtapaDict,
             tipoAprobadorDict);

        return plantilla;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static CrearSolicitudPlantilla? ObtenerFlujoDesdeConfiguracion(string? configuracion)
    {
        if (string.IsNullOrWhiteSpace(configuracion))
            return null;

        var flujo = JsonSerializer.Deserialize<CrearSolicitudPlantilla>(configuracion, JsonOptions);
        return flujo ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.ConfiguracionInvalid);
    }

    private async Task<Plantilla?> ObtenerPlantillaExistente(long idPlantilla, CancellationToken cancellationToken)
    {
        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(idPlantilla, cancellationToken, true);
        return plantilla;
    }

    private async Task<Plantilla> CrearDesdePlantillaMaestra(Plantilla plantillaMaestra, long idUsuario, string gerenciaUsuario, CancellationToken cancellationToken)
    {
        var flujo = ClonarPlantillaMaestra_a_FlujoInterno(plantillaMaestra);

        var tipoEtapaDict = await ObtenerDiccionarioTipoEtapa(cancellationToken);
        var tipoAprobadorDict = await ObtenerDiccionarioTipoAprobador(cancellationToken);

        long idPlantilla = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdPlantillaAsync(cancellationToken);
        long idEtapa = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdEtapaAsync(cancellationToken);
        long idFirmante = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdFirmanteAsync(cancellationToken);

        var requestMapper = MapearInputPlantilla(flujo);

        return PlantillaFactory.CrearDesdeRequest(
            requestMapper,
            idPlantilla,
            idEtapa,
            idFirmante,
            idUsuario,
            gerenciaUsuario,
            tipoEtapaDict,
            tipoAprobadorDict);
    }

    private async Task ActualizarPlantillaSiCorresponde(CrearSolicitudPlantilla? flujo, Plantilla plantilla, long idPlantillaRequest,
                long idUsuario, CancellationToken cancellationToken)
    {

        if (flujo is null)
            return;

        flujo.Id = idPlantillaRequest;

        await ActualizarPlantillaDesdeSolicitud(
            flujo,
            plantilla,
            idUsuario,
            cancellationToken);
    }

    private async Task<SolicitudFlujoDiff?> ActualizarPlantillaDesdeSolicitud(CrearSolicitudPlantilla request, Plantilla plantilla, long idUsuario, CancellationToken cancellationToken)
    {
        if (plantilla.EsPlantillaMaestra)
            return null;

        // Obtenemos el diccionario de tipo de etapas
        var tipoEtapaDict = await ObtenerDiccionarioTipoEtapa(cancellationToken);

        // Obtenemos el diccionario de tipo de aprobadores
        var tipoAprobadorDict = await ObtenerDiccionarioTipoAprobador(cancellationToken);

        var editPlantilla = MapearEditPlantilla(request);

        // Obtenemos los ID
        var cantidadNuevasEtapas = editPlantilla.EtapasRequest!.Count(f => f.Id == 0);
        var cantidadNuevosFirmantes = editPlantilla.EtapasRequest!.SelectMany(e => e.Firmantes!).Count(f => f.Id == 0);

        var rangoEtapas = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdEtapaAsync(cantidadNuevasEtapas, cancellationToken);
        var rangoFirmantes = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdFirmanteAsync(cantidadNuevosFirmantes, cancellationToken);

        var indexEtapa = 0; var indexFirmante = 0;
        Func<long> generarIdEtapa = () => rangoEtapas[indexEtapa++];
        Func<long> generarIdFirmante = () => rangoFirmantes[indexFirmante++];

        // Obtenemos los cambios que han mapeado en el flujo
        var diff = _flujoDiffDomainService.CalcularDiff(plantilla, editPlantilla);

        // Editamos el flujo del agregado Plantilla => Etapas y FirmanteFlujo
        plantilla.EditarFlujo(editPlantilla, generarIdEtapa, generarIdFirmante, tipoEtapaDict, tipoAprobadorDict, idUsuario);

        return diff;
    }

    private async Task<Dictionary<long, ParametroDetalle>> ObtenerDiccionarioTipoEtapa(CancellationToken cancellationToken)
    {
        var tipoEtapaDict = (await _parametroRepository
                .ObtenerDetallePorCodigoParametro("TIPO_ETAPA", cancellationToken))
                .ToDictionary(
                    x => x.Id,
                    x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
                );

        return tipoEtapaDict;
    }

    private async Task<Dictionary<long, ParametroDetalle>> ObtenerDiccionarioTipoAprobador(CancellationToken cancellationToken)
    {
        var tipoAprobadorDict =
         (await _parametroRepository.ObtenerDetallePorCodigoParametro("TIPO_APROBADOR", cancellationToken))
          .ToDictionary(
              x => x.Id,
              x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
          );

        return tipoAprobadorDict;
    }

    #endregion

    #region Mappings

    private static CrearPlantillaInput MapearInputPlantilla(CrearSolicitudPlantilla plantilla)
    {
        var input = new CrearPlantillaInput
        {
            NombrePlantilla = plantilla.NombrePlantilla,
            Descripcion = plantilla.Descripcion,
            Visibilidad = null,
            EsReutilizable = false,

            Etapas = plantilla.Etapas!
                        .Select(e => new EtapaInput
                        {

                            OrdenEtapa = e.OrdenEtapa,
                            NombreEtapa = e.NombreEtapa,
                            IdTipoEtapa = e.IdTipoEtapa,
                            IdTipoAprobador = e.IdTipoAprobador,

                            Firmantes = e.Firmantes!
                                .Select(f => new FirmanteInput
                                {
                                    IdUsuarioAsignado = f.IdUsuarioAsignado,
                                    CargoUsuario = f.CargoUsuario,
                                    AreaUsuario = f.AreaUsuario,
                                    GerenciaUsuario = f.GerenciaUsuario,
                                    IdUsuarioOrigen = f.IdUsuarioOrigen
                                })
                                .ToList()
                        })
                        .ToList()
        };

        return input;
    }

    private static CrearSolicitudPlantilla ClonarPlantillaMaestra_a_FlujoInterno(Plantilla request)
    {

        var edit = new CrearSolicitudPlantilla
        {
            Id = request.Id,
            NombrePlantilla = (request.NombrePlantilla == null ? null : request.NombrePlantilla.Valor),
            Descripcion = (request.Descripcion == null ? null : request.Descripcion.Valor),
            Visibilidad = null,
            EsReutilizable = false,
            Etapas = request.Etapas!.Select(e => new PlantillaEtapaRequest
            {
                OrdenEtapa = e.OrdenEtapa,
                NombreEtapa = e.NombreEtapa.Valor,
                IdTipoEtapa = e.IdTipoEtapa,
                IdTipoAprobador = e.IdTipoAprobador,
                Firmantes = e.Firmantes!.Select(f => new FirmanteFlujoRequest
                {
                    IdUsuarioAsignado = f.IdUsuarioAsignado,
                    CargoUsuario = f.CargoUsuario,
                    AreaUsuario = f.AreaUsuario,
                    GerenciaUsuario = f.GerenciaUsuario,
                    IdUsuarioOrigen = f.IdUsuarioOrigen
                }).ToList()
            }).ToList()
        };

        return edit;
    }

    private static PlantillaEditModel MapearEditPlantilla(CrearSolicitudPlantilla request)
    {
        var etapasCorregidas = request.Etapas!
             .Select(e =>
             {
                 if (e.TempId == Guid.Empty)
                     e.TempId = Guid.NewGuid();

                 return e;
             })
             .ToList();

        request.Etapas = etapasCorregidas;

        var edit = new PlantillaEditModel
        {
            Id = request.Id!.Value,
            EtapasRequest = request.Etapas!.Select(e => new EtapaEditModel
            {
                Id = e.Id == null ? 0 : e.Id.Value,
                TempId = e.TempId,
                Orden = e.OrdenEtapa,
                Nombre = e.NombreEtapa,
                IdTipoEtapa = e.IdTipoEtapa,
                IdTipoAprobador = e.IdTipoAprobador,
                Firmantes = e.Firmantes!.Select(f => new FirmanteEditModel
                {
                    Id = f.Id == null ? 0 : f.Id.Value,
                    IdUsuarioAsignado = f.IdUsuarioAsignado,
                    CargoUsuario = f.CargoUsuario,
                    AreaUsuario = f.AreaUsuario,
                    GerenciaUsuario = f.GerenciaUsuario,
                    IdUsuarioOrigen = f.IdUsuarioOrigen
                }).ToList()
            }).ToList()
        };

        return edit;
    }


    private static CrearSolicitudPlantilla ResetearIdsPlantillaMaestra(CrearSolicitudPlantilla request)
    {

        var edit = new CrearSolicitudPlantilla
        {
            Id = request.Id,
            NombrePlantilla = request.NombrePlantilla,
            Descripcion = request.Descripcion,
            Visibilidad = null,
            EsReutilizable = false,
            Etapas = request.Etapas!.Select(e => new PlantillaEtapaRequest
            {
                Id = 0,
                OrdenEtapa = e.OrdenEtapa,
                NombreEtapa = e.NombreEtapa,
                IdTipoEtapa = e.IdTipoEtapa,
                IdTipoAprobador = e.IdTipoAprobador,
                Firmantes = e.Firmantes!.Select(f => new FirmanteFlujoRequest
                {
                    Id = 0,
                    IdUsuarioAsignado = f.IdUsuarioAsignado,
                    CargoUsuario = f.CargoUsuario,
                    AreaUsuario = f.AreaUsuario,
                    GerenciaUsuario = f.GerenciaUsuario,
                    IdUsuarioOrigen = f.IdUsuarioOrigen
                }).ToList()
            }).ToList()
        };

        return edit;
    }
    #endregion

}
