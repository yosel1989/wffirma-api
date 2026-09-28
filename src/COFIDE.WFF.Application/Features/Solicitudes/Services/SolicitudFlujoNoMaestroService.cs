using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudFlujoNoMaestroService(
        ICurrentUserService currentUser,
        IParametroRepository parametroRepository,
        IWorkflowDomainService workflowDomainService,
        ISolicitudFlujoDiffService solicitudFlujoDiffService,
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository
    ) : ISolicitudFlujoNoMaestroService
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IParametroRepository _parametroRepository = parametroRepository;
    private readonly IWorkflowDomainService _workflowDomainService = workflowDomainService;
    private readonly ISolicitudFlujoDiffService _solicitudFlujoDiffService = solicitudFlujoDiffService;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;

    // Esto incluye el registro de plantilla --> etapas --> firmantes | solicitud plantilla --> firmantes Flujo --> Detalle Firma (si existe documentos)
    public async Task<Plantilla> CrearPlantillaDesdeSolicitud(CrearSolicitudPlantilla request, Solicitud solicitud, CancellationToken cancellationToken)
    {
        long idUSuario = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;

        // Obtenemos el diccionario de tipo de etapas
        var tipoEtapaDict = await ObtenerDiccionarioTipoEtapa(cancellationToken);

        // Obtenemos el diccionario de tipo de aprobadores
        var tipoAprobadorDict = await ObtenerDiccionarioTipoAprobador(cancellationToken);

        // Obtenemos los ID's de plantilla, etapa y firmantes
        long idPlantilla = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdPlantillaAsync(cancellationToken);
        long idEtapa = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdEtapaAsync(cancellationToken);
        long idFirmante = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdFirmanteAsync(cancellationToken);

        var requestMapper = MapearInputPlantilla(request);

        // Crearmos la plantilla
        var plantilla = PlantillaFactory.CrearDesdeRequest(
               requestMapper,
               idPlantilla,
               idEtapa,
               idFirmante,
               idUSuario,
               gerenciaUsuario,
               tipoEtapaDict,
               tipoAprobadorDict
           );
        return plantilla;
    }

    public async Task CrearFlujoDesdeSolicitud(Plantilla plantilla, Solicitud solicitud, CancellationToken cancellationToken)
    {
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

        await CrearListaDetalleFirma(solicitud, generarIdDetalleFirma);
    }

    // Esto incluye el registro de nuevas etapas, firmantes desde una plantilla ya creada --> etapas --> firmantes | solicitud plantilla --> firmantes Flujo --> Detalle Firma (si existe documentos)
    public async Task<SolicitudFlujoDiff?> ActualizarPlantillaDesdeSolicitud(CrearSolicitudPlantilla request, Solicitud solicitud, Plantilla plantilla, CancellationToken cancellationToken)
    {
        if (plantilla.EsPlantillaMaestra)
            return null;

        long idUSuario = _currentUser.UserIdLocal!.Value;

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
        var diff = _solicitudFlujoDiffService.CalcularDiff(plantilla, editPlantilla);

        // Editamos el flujo del agregado Plantilla => Etapas y FirmanteFlujo
        plantilla.EditarFlujo(editPlantilla, generarIdEtapa, generarIdFirmante, tipoEtapaDict, tipoAprobadorDict, idUSuario);

        return diff;
    }

    public async Task ActualizarFlujoDesdeSolicitud(Plantilla plantilla, Solicitud solicitud, CancellationToken cancellationToken)
    {
        // Obtenemos el diccionario de tipo de aprobadores
        var tipoAprobadorDict = await ObtenerDiccionarioTipoAprobador(cancellationToken);

        var cantidadNuevosFirmantes = plantilla.Etapas!.SelectMany(e => e.Firmantes!).Count();
        var cantidadNuevosDetalleFirma = (cantidadNuevosFirmantes * (solicitud.Documentos.Count));
        var rangoSolicitudFirmantes = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(cantidadNuevosFirmantes, cancellationToken);
        var rangoDetalleFirma = await _generadorCorrelativoIdRepository.ObtenerBloqueSiguienteIdDetalleFirmaAsync(cantidadNuevosDetalleFirma, cancellationToken);
        var indexSolicitudFirmante = 0; var indexDetalleFirma = 0;
        Func<long> generarIdSolicitudFirmante = () => rangoSolicitudFirmantes[indexSolicitudFirmante++];
        Func<long> generarIdDetalleFirma = () => rangoDetalleFirma[indexDetalleFirma++];

        // Seteamos el Usuarioasignado al firmante aún vigente
        _workflowDomainService.ConstruirFirmanteFlujoEtapa(solicitud, plantilla, tipoAprobadorDict);

        await CrearListaDetalleFirma(solicitud, generarIdDetalleFirma);

        _workflowDomainService.ActualizarSolicitudFirmanteNoMaestroFlujo(solicitud.SolicitudPlantilla, plantilla, generarIdSolicitudFirmante, generarIdDetalleFirma, null);
    }

    #region Métodos privados
    private static CrearPlantillaInput MapearInputPlantilla(CrearSolicitudPlantilla plantilla)
    {
        var input = new CrearPlantillaInput
        {
            NombrePlantilla = plantilla.NombrePlantilla,
            Descripcion = plantilla.Descripcion,
            Visibilidad = plantilla.Visibilidad,
            EsReutilizable = plantilla.EsReutilizable,

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

    private static Task CrearListaDetalleFirma(
    Solicitud solicitud,
    Func<long>? generarIdDetalleFirma)
    {
        if (generarIdDetalleFirma is null)
            throw new InvalidOperationException("Se requiere un generador de Id para DetalleFirma.");

        foreach (var solFirmaFlujo in solicitud.SolicitudPlantilla.FirmantesFlujo)
        {
            foreach (var documento in solicitud.Documentos)
            {
                var yaExiste = documento.DetallesFirma
                    .Any(x => x.IdSolicitudFirmanteFlujo == solFirmaFlujo.Id);

                if (yaExiste)
                    continue;

                var detalle = DetalleFirma.Crear(
                    generarIdDetalleFirma(),
                    solFirmaFlujo.Id,
                    documento.Id,
                    null,
                    null,
                    null,
                    null,
                    solFirmaFlujo.IdUsuarioAsignado
                );

                documento.AgregarDetalleFirma(detalle);
            }
        }
        return Task.CompletedTask;
    }
    #endregion
}
