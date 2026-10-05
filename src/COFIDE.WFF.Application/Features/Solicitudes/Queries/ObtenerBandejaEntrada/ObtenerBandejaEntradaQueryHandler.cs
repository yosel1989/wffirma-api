using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Common.Pageds;
using MediatR;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;

public sealed class ObtenerBandejaEntradaQueryHandler(
        ICurrentUserService currentUser,
        ISolicitudQueryRepository solicitudQueryRepository
    ) : IRequestHandler<ObtenerBandejaEntradaQuery, BandejaEntradaResponseDto>
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ISolicitudQueryRepository _solicitudQueryRepository = solicitudQueryRepository;
    public async Task<BandejaEntradaResponseDto> Handle(ObtenerBandejaEntradaQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var idUsuario = _currentUser.UserIdLocal;

        var paginationParams = new PagedQueryParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm
        };

        var pagedResult = await _solicitudQueryRepository.ObtenerBandejaEntrada(
          paginationParams,
          idUsuario,
          request.Fechainicio,
          request.Fechafin,
          request.Gerencia,
          request.Departamento,
          request.Solicitante,
          request.Tiposolicitud,
          request.Subtiposolicitud,
          request.TipoOperacion,
          request.Codigosolicitud,
          request.Asunto,
          request.Referencia,
          request.Urgente,
          request.IncluirResumen,
          cancellationToken);

        return new BandejaEntradaResponseDto
        {
            Total = pagedResult.ExtraData?.Total ?? 0,
            Urgente = pagedResult.ExtraData?.Urgente ?? 0,
            ListadoSolicitudes = pagedResult.Items,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }
}