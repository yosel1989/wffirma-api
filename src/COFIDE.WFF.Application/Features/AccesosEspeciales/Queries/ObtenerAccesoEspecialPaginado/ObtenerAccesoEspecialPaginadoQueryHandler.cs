using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
using MediatR;
using COFIDE.WFF.Application.Common.Pageds;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado
{
    public sealed class ObtenerAccesoEspecialPaginadoQueryHandler(
        ICurrentUserService currentUser,
        IAccesoEspecialQueryRepository queryRepository
    ) : IRequestHandler<ObtenerAccesoEspecialPaginadoQuery, AccesoEspecialPaginadoResponseDto>
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        private readonly IAccesoEspecialQueryRepository _queryRepository = queryRepository;
        public async Task<AccesoEspecialPaginadoResponseDto> Handle(ObtenerAccesoEspecialPaginadoQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserIdLocal is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            if (_currentUser.UserCargoId is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            var paginationParams = new PagedQueryParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SearchTerm = request.SearchTerm
            };

            var pagedResult = await _queryRepository.ObtenerPaginado(
                paginationParams,
                request.IdArea,
                request.IdUsuario,
                cancellationToken);

            var resumen = pagedResult.ExtraData!;

            return new AccesoEspecialPaginadoResponseDto
            {
                Total = resumen.Total,
                Items = pagedResult.Items,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
            };
        }
    }
}
