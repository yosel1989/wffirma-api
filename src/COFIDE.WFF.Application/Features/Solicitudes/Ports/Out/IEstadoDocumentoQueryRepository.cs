using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoDocumentosCombo;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IEstadoDocumentoQueryRepository
{
    Task<List<EstadoDocumentosComboDto>> GetEstadoDocumentoCombo(CancellationToken cancellationToken);
}

