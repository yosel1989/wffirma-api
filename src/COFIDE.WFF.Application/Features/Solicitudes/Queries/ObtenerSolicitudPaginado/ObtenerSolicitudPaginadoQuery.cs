using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;

public sealed record ObtenerSolicitudPaginadoQuery
(
    string? Fechainicio,
    string? Fechafin,
    List<long>? Gerencia,
    List<long>? Departamento,
    List<long>? Solicitante,
    List<long>? Tiposolicitud,
    List<long>? Subtiposolicitud,
    List<long>? TipoOperacion,
    List<long>? AprobadorActual,
    List<long>? Estado,
    string? Codigosolicitud,
    string? Asunto,
    string? Referencia,
    bool? Urgente,
    bool? Usuario,
    bool? Usuariodepartamento,
    bool? IncluirResumen
) : GetPagedQuery<SolicitudPaginadoResponseDto>;