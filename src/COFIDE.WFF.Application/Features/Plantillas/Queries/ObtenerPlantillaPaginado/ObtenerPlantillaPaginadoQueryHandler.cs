using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Domain.Common.Pageds;
using MediatR;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;

public sealed class ObtenerPlantillaPaginadoQueryHandler(
        IPlantillaRepository plantillaRepository,
        ICurrentUserService currentUser
    ) : IRequestHandler<ObtenerPlantillaPaginadoQuery, PagedResultDto<PlantillaPaginadoDto>>
{
    private readonly IPlantillaRepository _plantillaRepository = plantillaRepository;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<PagedResultDto<PlantillaPaginadoDto>> Handle(ObtenerPlantillaPaginadoQuery qry, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserIdLocal;
        var rolUusario = _currentUser.RolUsuario;
        string gerenciaUsuario = _currentUser.UserGerenciaNombre!;

        var paginationParams = new PagedQueryParams
        {
            PageNumber = qry.PageNumber,
            PageSize = qry.PageSize,
            SearchTerm = qry.SearchTerm
        };

        var request = new PlantillaPaginadoRequest
        {
            PaginationParams = paginationParams,
            Busqueda = qry.busqueda,
            Visibilidad = qry.Visibilidad,
            Aprobadores = qry.aprobadores,
            Gerencias = qry.gerencias,
            RolUsuario = rolUusario!.Value,
            AreaParaCoordinador = gerenciaUsuario,
            IdUsuarioPropietario = userId!.Value
        };

        var pagedResult = await _plantillaRepository.ObtenerPaginado(request);


        var pagedDto = new PagedResultDto<PlantillaPaginadoDto>(
            pagedResult.Items,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            pagedResult.TotalCount
        );

        return pagedDto;
    }
}
