using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Common.Pageds;
using MediatR;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerDocumentoBloqueadoPaginado;

public sealed class ObtenerDocumentoBloqueadoPaginadoQueryHandler(       
        ISolicitudDocumentoRepository solicitudDocumentoRepository
    ) : IRequestHandler<ObtenerDocumentoBloqueadoPaginadoQuery, PagedResultDto<DocumentoBloqueadoPaginadoDto>>
{
    private readonly ISolicitudDocumentoRepository _solicitudDocumentoRepository = solicitudDocumentoRepository;
    public async Task<PagedResultDto<DocumentoBloqueadoPaginadoDto>> Handle(ObtenerDocumentoBloqueadoPaginadoQuery request, CancellationToken cancellationToken)
    {       
        var paginationParams = new PagedQueryParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm
        };

        var pagedResult = await _solicitudDocumentoRepository.ObtenerGestionDocumentosBloqueadosPaginado(
            paginationParams,
            request.FechaInicio,
            request.FechaFin,
            request.NombreArchivo,
            request.CodigoSolicitud,
            request.FirmanteActual,
            cancellationToken);

        var pagedDto = new PagedResultDto<DocumentoBloqueadoPaginadoDto>(
          pagedResult.Items,
          paginationParams.PageNumber,
          paginationParams.PageSize,
          pagedResult.TotalCount
      );

        return pagedDto;
    }
}