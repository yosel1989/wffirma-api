using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;

public sealed class ValidarExistenciaQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IPlantillaRepository plantillaRepository
    ) : BaseQueryHandler<ValidarExistenciaQuery, ValidarExistenciaResponseDto?>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;

    public override async Task<ValidarExistenciaResponseDto?> Handle(ValidarExistenciaQuery request, CancellationToken cancellationToken)
    {
        ValidarExistenciaResponseDto response;
        long usuarioActual = _currentUser.UserIdLocal!.Value;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;
        var rolUusario = _currentUser.RolUsuario;
        var usuario = new Usuario(usuarioActual, rolUusario!.Value);

        var plantilla = await _plantillaRepository.ValidarExistenciaNombrePlantilla(request.NombrePlantilla, request.Visibilidad, usuarioActual);

        if (plantilla == null)
        {
            response = new ValidarExistenciaResponseDto
            {
                Existe = false,
                IdPlantilla = 0,
            };
        }
        else
        {
            bool validar = plantilla.ValidarPermisoPlantilla(usuarioActual, gerenciaUsuario, usuario);

            response = new ValidarExistenciaResponseDto
            {
                Existe = true,
                IdPlantilla = plantilla.Id,
                Mensaje = validar ? "Existe una plantilla con el mismo nombre. ¿Desea reemplazar o renombrar?" : "Existe una plantilla con el mismo nombre. ¿Desea renombrarla?",
                BotonPrimario = "Renombrar",
                BotonSecundario = validar ? "Reemplazar" : "Cancelar"
            };
        }

        return response;
    }
}