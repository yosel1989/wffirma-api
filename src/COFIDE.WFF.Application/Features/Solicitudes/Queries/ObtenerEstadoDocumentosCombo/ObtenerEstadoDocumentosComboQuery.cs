using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerEstadoDocumentosCombo;

public sealed record ObtenerEstadoDocumentosComboQuery() : BaseQuery<List<EstadoDocumentosComboDto>>;