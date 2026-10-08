using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Application.Common.Constants;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;

public sealed class EditarPlantillaCommandHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IPlantillaRepository plantillaRepository,
        IParametroRepository parametroRepository
    ) : BaseCommandHandler<EditarPlantillaCommand, long>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly IParametroRepository _parametroRepository = parametroRepository;

    public override async Task<long> Handle(EditarPlantillaCommand request, CancellationToken cancellationToken)
    {
        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;
        var rolUusario = _currentUser.RolUsuario;
        var usuario = new Usuario(usuarioActual, rolUusario!.Value);

        var plantilla = await _plantillaRepository.ObtenerAgregadoPlantillaAsync(request.Id, cancellationToken, true)
        ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.PlantillaInvalid);

        plantilla.PermitirActualizarPlantilla(usuarioActual, gerenciaUsuario, usuario);

        var tipoAprobadorDict = (await _parametroRepository
           .ObtenerDetallePorCodigoParametro(CodigosParametro.TipoAprobador, cancellationToken))
           .ToDictionary(
               x => x.Id,
               x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
           );

        var tipoEtapaDict = (await _parametroRepository
              .ObtenerDetallePorCodigoParametro(CodigosParametro.TipoEtapa, cancellationToken))
              .ToDictionary(
                  x => x.Id,
                  x => new ParametroDetalle(x.Id, x.Codigo, x.Valor)
              );

        var editPlantilla = EditarPlantillaMapping.MapearEditPlantilla(request);

        var contadorEtapasNoExistentes = editPlantilla.EtapasRequest.Count(etapaReq =>
           etapaReq.Id != 0 &&
           !plantilla.Etapas.Any(e =>
               e.Id == etapaReq.Id &&
               e.Estado));

        var cantidadNuevosFirmantes = editPlantilla.EtapasRequest.Sum(etapaReq =>
        {
            bool etapaExiste = etapaReq.Id != 0 &&
                plantilla.Etapas.Any(e =>
                    e.Id == etapaReq.Id &&
                    e.Estado);

            // Si la etapa no existe, todos los firmantes son nuevos
            if (!etapaExiste)
            {
                return etapaReq.Firmantes?.Count ?? 0;
            }

            // Si la etapa existe, validar firmantes individualmente
            var etapaPlantilla = plantilla.Etapas
                .First(e => e.Id == etapaReq.Id && e.Estado);

            return etapaReq.Firmantes!.Count(firmanteReq =>
                firmanteReq.Id == 0 ||
                !etapaPlantilla.Firmantes.Any(fp =>
                    fp.Id == firmanteReq.Id &&
                    fp.Estado));
        });

        var cantidadNuevasEtapas = editPlantilla.EtapasRequest!.Count(f => f.Id == 0) + contadorEtapasNoExistentes;
        var rangoEtapas = await _plantillaRepository.ObtenerBloqueSiguienteIdEtapaAsync(cantidadNuevasEtapas, cancellationToken);
        var rangoFirmantes = await _plantillaRepository.ObtenerBloqueSiguienteIdFirmanteAsync(cantidadNuevosFirmantes, cancellationToken);

        var indexEtapa = 0; var indexFirmante = 0;
        Func<long> generarIdEtapa = () => rangoEtapas[indexEtapa++];
        Func<long> generarIdFirmante = () => rangoFirmantes[indexFirmante++];

        plantilla.EditarFlujo(editPlantilla, generarIdEtapa, generarIdFirmante, tipoEtapaDict, tipoAprobadorDict, usuarioActual);
        _plantillaRepository.GuardarFlujoPlantillaAsync(plantilla);
        return plantilla.Id;
    }
}
