using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Common.Pageds;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;

public sealed class ObtenerSolicitudPaginadoQueryHandler(
        ICurrentUserService currentUser,
        ISolicitudQueryRepository solicitudQueryRepository
    ) : IRequestHandler<ObtenerSolicitudPaginadoQuery, SolicitudPaginadoResponseDto>
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    public async Task<SolicitudPaginadoResponseDto> Handle(ObtenerSolicitudPaginadoQuery request, CancellationToken cancellationToken)
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

        var pagedResult = await _solicitudQueryRepository.ObtenerPaginado(
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
            request.Tiposolicitud,
            request.Subtiposolicitud,
            request.TipoOperacion,
            request.AprobadorActual,
            request.Estado,
            request.Codigosolicitud,
            request.Asunto,
            request.Referencia,
            request.Urgente,
            request.Usuario,
            request.Usuariodepartamento,
            request.IncluirResumen,
            cancellationToken);

        var resumen = pagedResult.ExtraData!;

        return new SolicitudPaginadoResponseDto
        {
            MisSolicitudes = resumen.MisSolicitudes,
            Total = resumen.Total,
            MiDepartamento = resumen.MiDepartamento,
            Urgente = resumen.Urgente,
            ListadoSolicitudItem = pagedResult.Items,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }
}
