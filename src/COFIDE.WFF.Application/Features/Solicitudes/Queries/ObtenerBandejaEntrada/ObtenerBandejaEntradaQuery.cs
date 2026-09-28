using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
public sealed record ObtenerBandejaEntradaQuery(
    string? Fechainicio,
    string? Fechafin,
    List<long>? Gerencia,
    List<long>? Departamento,
    List<long>? Solicitante,
    List<long>? Tiposolicitud,
    List<long>? Subtiposolicitud,
    List<long>? TipoOperacion,
    string? Codigosolicitud,
    string? Asunto,
    string? Referencia,
    bool? Urgente,
    bool? IncluirResumen
) : GetPagedQuery<BandejaEntradaResponseDto>;