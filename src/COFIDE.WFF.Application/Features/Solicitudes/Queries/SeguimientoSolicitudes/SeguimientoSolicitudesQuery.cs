using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;

public sealed record SeguimientoSolicitudesQuery(
    string? Fechainicio,
    string? Fechafin,
    List<long>? Gerencia,
    List<long>? Departamento,
    List<long>? Solicitante,
    List<long>? Tiposolicitud,
    List<long>? Subtiposolicitud,
    List<long>? TipoOperacion,
    string? Asunto,
    string? Referencia,
    List<long>? Estado,
    List<long>? Firmante,
    string? Codigosolicitud,
    bool? Encurso,
    bool? Aprobada,
    bool? Rechazada,
    bool? IncluirResumen
    ) : GetPagedQuery<SeguimientoSolicitudesResponseDto>;
