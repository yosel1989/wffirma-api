using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Services
{
    /// <summary>
    /// Servicio encargado de registrar solicitudes especiales de negocio.
    /// </summary>
    public interface IAccesoEspecialRegistrarService
    {
        /// <summary>
        /// Crea una nueva solicitud de acceso especial con la información proporcionada.
        /// </summary>
        /// <param name="request">Objeto que contiene los datos necesarios para registrar la solicitud de acceso especial.</param>
        /// <param name="cancellationToken">Token para manejar la cancelación de la operación.</param>
        /// <returns>Una tarea que representa la operación asincrónica. El resultado contiene la instancia de la solicitud de acceso especial creada.</returns>
        AccesoEspecial AccesoEspecialRegistrar(
            AccesoEspecialRequestDto request,
            CancellationToken cancellationToken);
    }
}
