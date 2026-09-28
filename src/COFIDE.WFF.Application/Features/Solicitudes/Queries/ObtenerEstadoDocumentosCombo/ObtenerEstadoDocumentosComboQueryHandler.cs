using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoDocumentosCombo;

public sealed class ObtenerEstadoDocumentosComboQueryHandler(
        IMapper mapper,
        IEstadoDocumentoQueryRepository estadoDocumentoQueryRepository
    ) : BaseQueryHandler<ObtenerEstadoDocumentosComboQuery, List<EstadoDocumentosComboDto>>(mapper)
{
    private readonly IEstadoDocumentoQueryRepository _estadoDocumentoQueryRepository = estadoDocumentoQueryRepository;

    public override async Task<List<EstadoDocumentosComboDto>> Handle(
        ObtenerEstadoDocumentosComboQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _estadoDocumentoQueryRepository.GetEstadoDocumentoCombo(cancellationToken);
        return dtoItems;
    }
}
