using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes
{
    public sealed class ObtenerGestionSolicitudesQueryHandler(
        ICurrentUserService currentUser,
        ISolicitudGestionRepository solicitudQueryRepository
    ) : IRequestHandler<ObtenerGestionSolicitudesQuery, ObtenerGestionSolicitudesResponseDto>
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        private readonly ISolicitudGestionRepository _solicitudQueryRepository = solicitudQueryRepository;

        public async Task<ObtenerGestionSolicitudesResponseDto> Handle(ObtenerGestionSolicitudesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserIdLocal is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);
            if (_currentUser.UserCargoId is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            var idUsuario = _currentUser.UserIdLocal;
            var areaUsuario = _currentUser.UserAreaId;
            var rolUsuario = _currentUser.CodigoRolUsuario;
            var idDepartamento = _currentUser.UserAreaId;

            var paginationParams = new PagedQueryParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SearchTerm = request.SearchTerm
            };

            var pagedResult = await _solicitudQueryRepository.ObtenerPaginadoGestionSeguimiento(
                paginationParams,
                idUsuario,
                areaUsuario,
                rolUsuario,
                idDepartamento,
                request.Fechainicio,
                request.Fechafin,
                request.Gerencia,
                request.Departamento,
                request.Solicitante,
                request.Estado,
                request.Firmante,
                request.Codigosolicitud,
                cancellationToken);

            var resumen = pagedResult.ExtraData!;

            return new ObtenerGestionSolicitudesResponseDto
            {
                Total = resumen.Total,
                ListadoSolicitudes = pagedResult.Items,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
            };
        }

    }
}
