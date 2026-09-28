using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Factories;

public static class PlantillaFactory
{
    public static Plantilla CrearDesdeRequest(
        CrearPlantillaInput request,
        long idPlantilla,
        long idEtapaInicial,
        long idFirmanteInicial,
        long usuarioActual,
        string gerenciaUsuario,
        Dictionary<long, ParametroDetalle> tipoEtapaDict,
        Dictionary<long, ParametroDetalle> tipoAprobadorDict)
    {

        var plantilla = Plantilla.Crear(
                idPlantilla,
                request.NombrePlantilla,
                request.Descripcion,
                request.Visibilidad,
                usuarioActual,
                gerenciaUsuario,
                request.EsReutilizable
           );

        plantilla.MarcarComoNuevo();

        long idEtapa = idEtapaInicial;
        long idFirmante = idFirmanteInicial;

        if (request.Etapas is null || request.Etapas.Count == 0)
            return plantilla;

        foreach (var etapaRequest in request.Etapas)
        {
            var tipoEtapa = Vo.TipoEtapa.ObtenerTipoEtapa(etapaRequest.IdTipoEtapa, tipoEtapaDict);
            var tipoAprobador = Vo.TipoAprobador.ObtenerTipoAprobador(etapaRequest.IdTipoAprobador, tipoAprobadorDict);

            var etapa = plantilla.AgregarEtapas(
                idEtapa++,
                etapaRequest.OrdenEtapa,
                etapaRequest.NombreEtapa,
                tipoEtapa,
                etapaRequest.IdTipoEtapa,
                tipoAprobador,
                etapaRequest.IdTipoAprobador
            );

            etapa.MarcarComoNuevo();

            if (etapaRequest.Firmantes is null) continue;

            foreach (var firmante in etapaRequest.Firmantes)
            {
                etapa.AgregarFirmante(
                    idFirmante++,
                    firmante.IdUsuarioAsignado,
                    firmante.CargoUsuario,
                    firmante.AreaUsuario,
                    firmante.GerenciaUsuario,
                    firmante.IdUsuarioOrigen
                );
            }
        }
        plantilla.ValidarFlujoCompleto();
        return plantilla;
    }
}
