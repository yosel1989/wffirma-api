using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

/// <summary>
/// Servicio encargado de crear solicitudes de negocio.
/// </summary>
public interface ISolicitudCreacionService
{
    /// <summary>
    /// Crea una nueva solicitud con la información proporcionada.
    /// </summary>
    /// <param name="input">Datos necesarios para registrar la solicitud.</param>
    /// <param name="codigoArea">Código del área del usuario solicitante.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Instancia de la solicitud creada.</returns>
    Task<Solicitud> CrearSolicitudBase(
        SolicitudRequestDto request,
        string codigoArea,
        long idUSuario,
        long idArea,
        long idAreaPadre,
        CancellationToken cancellationToken);
}
