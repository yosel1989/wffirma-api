using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;

public sealed class GestionPlantillasQueryHandler(
    ICurrentUserService currentUser,
    IPlantillaGestionRepository plantillaGestionRepository
) : IRequestHandler<GestionPlantillasQuery, GestionPlantillasResponseDto>
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IPlantillaGestionRepository _plantillaGestionRepository = plantillaGestionRepository;

    public async Task<GestionPlantillasResponseDto> Handle(GestionPlantillasQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);
        if (_currentUser.UserCargoId is null)
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

        var pagedResult = await _plantillaGestionRepository.ObtenerPaginadoGestion(
            paginationParams,
            _currentUser.UserIdLocal,
            _currentUser.UserAreaId,
            _currentUser.CodigoRolUsuario,
            request.Fechainicio,
            request.Fechafin,
            request.Gerencia,
            request.Departamento,
            request.Creador,
            request.Aprobador,
            request.NombrePlantilla,
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
