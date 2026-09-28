using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudFlujoDiff
{
    public List<EtapaAgregada> EtapasAgregadas { get; set; } = new();
    public List<EtapaEliminada> EtapasEliminadas { get; set; } = new();

    public List<FirmanteAgregado> FirmantesAgregados { get; set; } = new();
    public List<FirmanteEliminado> FirmantesEliminados { get; set; } = new();

    public List<EtapaMovida> EtapasMovidas { get; set; } = new();
}

public record EtapaAgregada(long Orden, long IdEtapa, string Nombre, Guid TempId);
public record EtapaEliminada(long Orden, long IdEtapa, string Nombre);

public record FirmanteAgregado(long IdUsuario, long IdEtapa, Guid TempIdEtapa, string NombreEtapa);
public record FirmanteEliminado(long IdUsuario, long IdEtapa, string NombreEtapa);

public record EtapaMovida(long IdEtapa, long OrdenAnterior, long OrdenNuevo, string Nombre);


public class SolicitudFlujoDiffService : ISolicitudFlujoDiffService
{
    public SolicitudFlujoDiff CalcularDiff(Plantilla plantilla, PlantillaEditModel request)
    {
        var diff = new SolicitudFlujoDiff();

        var etapasActuales = plantilla.Etapas
            .Where(e => e.Estado)
            .ToDictionary(e => e.Id);

        var etapasRequest = request.EtapasRequest
            .Where(e => e.Id != 0)
            .ToDictionary(e => e.Id);

        // ← cada bloque extraído a su método privado
        DetectarEtapasEliminadas(diff, etapasActuales, etapasRequest);
        DetectarEtapasYFirmantesAgregados(diff, request, etapasActuales);
        DetectarFirmantesModificados(diff, etapasActuales, etapasRequest);
        DetectarEtapasMovidas(diff, etapasActuales, etapasRequest);

        return diff;
    }

    private static void DetectarEtapasEliminadas(
        SolicitudFlujoDiff diff,
        Dictionary<long, PlantillaEtapa> etapasActuales,
        Dictionary<long, EtapaEditModel> etapasRequest)
    {
        foreach (var etapa in etapasActuales.Values)
        {
            if (!etapasRequest.ContainsKey(etapa.Id))
            {
                diff.EtapasEliminadas.Add(new EtapaEliminada(
                    etapa.OrdenEtapa,
                    etapa.Id,
                    etapa.NombreEtapa.Valor
                ));

                foreach (var firmante in etapa.Firmantes.Where(f => f.Estado))
                {
                    diff.FirmantesEliminados.Add(new FirmanteEliminado(
                        firmante.IdUsuarioAsignado,
                        etapa.Id,
                        etapa.NombreEtapa.Valor
                    ));
                }
            }
        }
    }

    private static void DetectarEtapasYFirmantesAgregados(
        SolicitudFlujoDiff diff,
        PlantillaEditModel request,
        Dictionary<long, PlantillaEtapa> etapasActuales)
    {
        foreach (var etapaReq in request.EtapasRequest)
        {
            if (etapaReq.Id == 0 || !etapasActuales.ContainsKey(etapaReq.Id))
            {
                diff.EtapasAgregadas.Add(new EtapaAgregada(
                    etapaReq.Orden,
                    0,
                    etapaReq.Nombre,
                    etapaReq.TempId
                ));

                foreach (var f in etapaReq.Firmantes)
                {
                    diff.FirmantesAgregados.Add(new FirmanteAgregado(
                        f.IdUsuarioAsignado,
                        0,
                        etapaReq.TempId,
                        etapaReq.Nombre
                    ));
                }
            }
        }
    }
    private static void DetectarFirmantesModificados(
        SolicitudFlujoDiff diff,
        Dictionary<long, PlantillaEtapa> etapasActuales,
        Dictionary<long, EtapaEditModel> etapasRequest)
    {
        foreach (var etapa in etapasActuales.Values)
        {
            if (!etapasRequest.TryGetValue(etapa.Id, out var etapaReq))
                continue;

            var actuales = etapa.Firmantes
                .Where(f => f.Estado)
                .ToDictionary(f => f.IdUsuarioAsignado);

            var nuevos = etapaReq.Firmantes
                .GroupBy(f => f.IdUsuarioAsignado)
                .ToDictionary(g => g.Key, g => g.First());

            // eliminados
            foreach (var f in actuales.Values)
            {
                if (!nuevos.ContainsKey(f.IdUsuarioAsignado))
                {
                    diff.FirmantesEliminados.Add(new FirmanteEliminado(
                        f.IdUsuarioAsignado,
                        etapa.Id,
                        etapa.NombreEtapa.Valor
                    ));
                }
            }

            // agregados
            foreach (var f in etapaReq.Firmantes)
            {
                if (!actuales.ContainsKey(f.IdUsuarioAsignado))
                {
                    diff.FirmantesAgregados.Add(new FirmanteAgregado(
                        f.IdUsuarioAsignado,
                        etapa.Id,
                        Guid.Empty,
                        etapa.NombreEtapa.Valor
                    ));
                }
            }
        }
    }
    private static void DetectarEtapasMovidas(
        SolicitudFlujoDiff diff,
        Dictionary<long, PlantillaEtapa> etapasActuales,
        Dictionary<long, EtapaEditModel> etapasRequest)
    {
        foreach (var etapa in etapasActuales.Values)
        {
            if (!etapasRequest.TryGetValue(etapa.Id, out var etapaReq))
                continue;

            if (etapa.OrdenEtapa != etapaReq.Orden)
            {
                diff.EtapasMovidas.Add(new EtapaMovida(
                    etapa.Id,
                    etapa.OrdenEtapa,
                    etapaReq.Orden,
                    etapa.NombreEtapa.Valor
                ));
            }
        }
    }
}