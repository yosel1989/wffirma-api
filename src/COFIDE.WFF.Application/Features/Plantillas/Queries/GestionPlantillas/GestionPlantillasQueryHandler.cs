using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.DomainServices;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;

public sealed class GestionPlantillasQueryHandler(
    ICurrentUserService currentUser,
    IPlantillaGestionRepository plantillaGestionRepository,
    IAccesoEspecialRepository accesoEspecialRepository
) : IRequestHandler<GestionPlantillasQuery, GestionPlantillasResponseDto>
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaGestionRepository _plantillaGestionRepository = plantillaGestionRepository;
    private readonly IAccesoEspecialRepository _accesoEspecialRepository = accesoEspecialRepository;

    public async Task<GestionPlantillasResponseDto> Handle(GestionPlantillasQuery request, CancellationToken cancellationToken)
    {
        List<long>? idAreasAccesoEspecial = new List<long>();

        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var paginationParams = new PagedQueryParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            Sorts = request.Sorts?
                .Where(s => !string.IsNullOrWhiteSpace(s.PropertyName))
                .Select(s => (s.PropertyName, s.Descending))
                .ToList()
        };

        var codigoUsuario = _currentUser.UserIdLocal;
        var departamentoUsuario = _currentUser.UserAreaId;
        var gerenciaUsuario = _currentUser.UserAreaPadreId;
        var codigoRolUsuario = _currentUser.CodigoRolUsuario;

        if (ReglaAccesoEspecial.EvaluarRolAccesoEspecial(codigoRolUsuario))
        {
            idAreasAccesoEspecial = await _accesoEspecialRepository.GetAreasByUserIdAsync(codigoUsuario.Value);
        }

        var pagedResult = await _plantillaGestionRepository.ObtenerPaginadoGestion(
            paginationParams,
            codigoUsuario,
            departamentoUsuario,
            gerenciaUsuario,
            codigoRolUsuario,
            request.Fechainicio,
            request.Fechafin,
            request.Gerencia,
            request.Departamento,
            request.Creador,
            request.Aprobador,
            request.NombrePlantilla,
            idAreasAccesoEspecial,
            cancellationToken);

        return new GestionPlantillasResponseDto
        {
            ListadoPlantillas = pagedResult.Items,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }
}
