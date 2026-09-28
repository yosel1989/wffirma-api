using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
namespace COFIDE.WFF.Domain.DomainServices;
public class WorkflowDomainService : IWorkflowDomainService
{
    public Solicitud ConstruirFirmanteFlujoEtapa(Solicitud solicitud, Plantilla plantilla, Dictionary<long, ParametroDetalle> tipoAprobadorDict)
    {
        foreach (var item in plantilla.Etapas.OrderBy(x => x.OrdenEtapa))
        {
            foreach (var item2 in item.Firmantes)
            {
                foreach (var item3 in solicitud.SolicitudPlantilla.FirmantesFlujo)
                {
                    if (item3.IdPlantillaEtapa == item.Id && item3.IdFirmanteFlujo == item2.Id)
                    {
                        var tipoAprobador = Vo.TipoAprobador.ObtenerTipoAprobador(item.IdTipoAprobador, tipoAprobadorDict);

                        item3.AsignarDatosEtapa(item.OrdenEtapa, item2.IdUsuarioAsignado, tipoAprobador.EsFirmante, tipoAprobador.EsVisador);
                    }
                }
            }
        }
        return solicitud;
    }

    public void AsignarUsuarioAsignado(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, Dictionary<long, ParametroDetalle> tipoAprobadorDict)
    {
        foreach (var item in plantilla.Etapas.OrderBy(x => x.OrdenEtapa))
        {
            foreach (var item2 in item.Firmantes)
            {
                foreach (var item3 in solicitudPlantilla.FirmantesFlujo)
                {
                    if (item3.IdPlantillaEtapa == item.Id && item3.IdFirmanteFlujo == item2.Id)
                    {
                        var tipoAprobador = Vo.TipoAprobador.ObtenerTipoAprobador(item.IdTipoAprobador, tipoAprobadorDict);

                        item3.AsignarDatosEtapa(item.OrdenEtapa, item2.IdUsuarioAsignado, tipoAprobador.EsFirmante, tipoAprobador.EsVisador);
                    }
                }
            }
        }
    }

    public void ValidacionFirmanteNoMaestroFlujo(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, PlantillaEditModel request)
    {
        var etapasCerradas = solicitudPlantilla.FirmantesFlujo
            .Where(x => x.Estado)
            .GroupBy(x => x.IdPlantillaEtapa)
            .Where(g => g.All(f => f.RevAprobado))
            .Select(g => g.Key)
            .ToHashSet();

        foreach (var etapaId in etapasCerradas)
        {
            var etapaActual = plantilla.Etapas
                .First(e => e.Id == etapaId && e.Estado);

            var etapaRequest = request.EtapasRequest
                .FirstOrDefault(e => e.Id == etapaId);

            if (etapaRequest is null)
                throw new DomainException(PlantillaErrors.EliminarEtapaAprobada);

            if (etapaActual.OrdenEtapa != etapaRequest.Orden)
                throw new DomainException(PlantillaErrors.CambiarOrdenEtapaAprobada);

            if (etapaActual.IdTipoEtapa != etapaRequest.IdTipoEtapa)
                throw new DomainException(PlantillaErrors.CambiarTipoEtapaEtapaAprobada);

            if (CambioFirmantes(etapaActual, etapaRequest))
                throw new DomainException(PlantillaErrors.ModificarFirmanteEtapaAprobada);
        }
    }

    private static bool CambioFirmantes(PlantillaEtapa actual, EtapaEditModel request)
    {
        var actuales = actual.Firmantes
            .Where(f => f.Estado)
            .Select(f => f.IdUsuarioAsignado)
            .OrderBy(x => x)
            .ToList();

        var nuevos = request.Firmantes
            .Select(f => f.IdUsuarioAsignado)
            .OrderBy(x => x)
            .ToList();

        return !actuales.SequenceEqual(nuevos);
    }

    public void ActualizarSolicitudFirmanteMaestroFlujo(SolicitudPlantilla solicitudPlantilla, Plantilla plantilla, long ultimoIdFirmanteFlujo)
    {
        var flujoDict = solicitudPlantilla.FirmantesFlujo
            .ToDictionary(x => x.IdUsuarioAsignado);

        foreach (var etapa in plantilla.Etapas.OrderBy(x => x.OrdenEtapa))
        {
            foreach (var firmante in etapa.Firmantes)
            {
                var userId = firmante.IdUsuarioAsignado;

                if (flujoDict.TryGetValue(userId, out var existente))
                {
                    existente.ActualizarDatosSolicitudFirmante(
                        firmante.Id,
                        existente.IdSolicitudPlantilla,
                        etapa.Id,
                        etapa.OrdenEtapa
                    );

                    flujoDict.Remove(userId);
                }
                else
                {
                    solicitudPlantilla.AgregarFirmanteFlujo(ultimoIdFirmanteFlujo++, firmante.Id, etapa.Id, etapa.OrdenEtapa, firmante.IdUsuarioAsignado);
                }
            }
        }
    }

    public void ActualizarSolicitudFirmanteNoMaestroFlujo(
        SolicitudPlantilla solicitudPlantilla,
        Plantilla plantilla,
        Func<long>? generarIdSolicitudFirmante = null,
        Func<long>? generarIdDetalleFirma = null,
        long? ultimoIdSolicitudFirmante = null)
    {
        var flujoDict = solicitudPlantilla.FirmantesFlujo
            .Where(x => x.Estado)
            .ToDictionary(x => x.IdUsuarioAsignado);

        foreach (var etapa in plantilla.Etapas.Where(x => x.Estado).OrderBy(x => x.OrdenEtapa))
        {
            foreach (var firmante in etapa.Firmantes.Where(x => x.Estado))
            {
                ProcesarFirmante(solicitudPlantilla, etapa, firmante, flujoDict, generarIdSolicitudFirmante, generarIdDetalleFirma);
            }
        }

        foreach (var sobrante in flujoDict.Values)
        {
            DesactivarSobrante(solicitudPlantilla, sobrante);
        }
    }

    private static void ProcesarFirmante(
        SolicitudPlantilla solicitudPlantilla,
        PlantillaEtapa etapa,
        FirmanteFlujo firmante,
        Dictionary<long, SolicitudFirmanteFlujo> flujoDict,
        Func<long>? generarIdSolicitudFirmante,
        Func<long>? generarIdDetalleFirma)
    {
        var userId = firmante.IdUsuarioAsignado;

        if (flujoDict.TryGetValue(userId, out var existente))
        {
            // No tocar los aprobados
            if (existente.RevAprobado)
            {
                flujoDict.Remove(userId);
                return;
            }

            existente.ActualizarDatosSolicitudFirmante(
                firmante.Id,
                existente.IdSolicitudPlantilla,
                etapa.Id,
                etapa.OrdenEtapa
            );
            flujoDict.Remove(userId);
        }
        else
        {
            if (generarIdSolicitudFirmante is null)
                throw new InvalidOperationException("generarIdSolicitudFirmante no fue inicializado.");

            long ultimoIdSolicituFirmante = generarIdSolicitudFirmante();
            solicitudPlantilla.AgregarFirmanteFlujo(ultimoIdSolicituFirmante, firmante.Id, etapa.Id, etapa.OrdenEtapa, firmante.IdUsuarioAsignado);

            foreach (var documento in solicitudPlantilla.Solicitud.Documentos)
            {
                if (generarIdDetalleFirma is null)
                    throw new InvalidOperationException("generarIdDetalleFirma no fue inicializado.");

                var detalle = DetalleFirma.Crear(
                    generarIdDetalleFirma(),
                    ultimoIdSolicituFirmante,
                    documento.Id,
                    null, null, null, null,
                    userId
                );
                documento.AgregarDetalleFirma(detalle);
            }
        }
    }

    private static void DesactivarSobrante(
        SolicitudPlantilla solicitudPlantilla,
        SolicitudFirmanteFlujo sobrante)
    {
        solicitudPlantilla.DesactivarFirmanteFlujo(sobrante);

        foreach (var documento in solicitudPlantilla.Solicitud.Documentos)
        {
            documento.DesactivarDetalleFirmaPorFirmante(sobrante.IdUsuarioAsignado);
        }
    }


    public void AgregarFirmantesFlujoDesdeEtapas(
      Solicitud solicitud,
      Plantilla plantilla,
      Func<long> generarIdSolicitudFirmante)
    {
        foreach (var item in plantilla.Etapas)
        {
            foreach (var item2 in item.Firmantes)
            {
                solicitud.SolicitudPlantilla.AgregarFirmanteFlujo(
                    generarIdSolicitudFirmante(),
                    item2.Id,
                    item2.IdPlantillaEtapa,
                    item.OrdenEtapa,
                    item2.IdUsuarioAsignado);
            }
        }
    }
}