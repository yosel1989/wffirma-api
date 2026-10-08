using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

/// <summary>Registra una solicitud completa (correlativos, documentos, sustentos y plantilla).
/// Lo comparten los handlers de registrar y de registrar y enviar.</summary>
public interface ISolicitudRegistroService
{
    Task<(Solicitud, Plantilla?)> Crear(SolicitudRequestDto cmd, bool esBorrador, CancellationToken cancellationToken);
}
