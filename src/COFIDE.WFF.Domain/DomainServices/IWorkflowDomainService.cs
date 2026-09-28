using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Domain.DomainServices;

public interface IWorkflowDomainService
{
    Solicitud ConstruirFirmanteFlujoEtapa(Solicitud solicitud, Plantilla plantilla, Dictionary<long, ParametroDetalle> tipoAprobadorDict);
    void AsignarUsuarioAsignado(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, Dictionary<long, ParametroDetalle> tipoAprobadorDict);
    void ValidacionFirmanteNoMaestroFlujo(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, PlantillaEditModel request);
    void ActualizarSolicitudFirmanteMaestroFlujo(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, long ultimoIdFirmanteFlujo);
    void ActualizarSolicitudFirmanteNoMaestroFlujo(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, Func<long>? generarIdSolicitudFirmante = null,
        Func<long>? generarIdDetalleFirma = null, long? ultimoIdSolicitudFirmante = null);

    void AgregarFirmantesFlujoDesdeEtapas(Solicitud solicitud, Plantilla plantilla, Func<long> generarIdSolicitudFirmante);
}
