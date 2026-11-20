using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Entities;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Factories;

public static class PlantillaFactory
{
    /// <summary>
    /// Crea una plantilla desde una solicitud y el contexto del usuario.
    /// </summary>
    /// <param name="request">Datos de creación de la plantilla.</param>
    /// <param name="idPlantilla">Identificador de la plantilla.</param>
    /// <param name="idEtapaInicial">Identificador inicial de etapa.</param>
    /// <param name="idFirmanteInicial">Identificador inicial de firmante.</param>
    /// <param name="usuarioActual">Identificador del usuario propietario.</param>
    /// <param name="gerenciaUsuario">Gerencia del usuario propietario.</param>
    /// <param name="tipoEtapaDict">Tipos de etapa disponibles.</param>
    /// <param name="tipoAprobadorDict">Tipos de aprobador disponibles.</param>
    /// <param name="idAreaPlantilla">Identificador del área, si está disponible.</param>
    /// <param name="idAreaPadrePlantilla">Identificador del área padre, si está disponible.</param>
    /// <returns>La plantilla creada.</returns>
    public static Plantilla CrearDesdeRequest(
        CrearPlantillaInput request,
        long idPlantilla,
        long idEtapaInicial,
        long idFirmanteInicial,
        long usuarioActual,
        string gerenciaUsuario,
        Dictionary<long, ParametroDetalle> tipoEtapaDict,
        Dictionary<long, ParametroDetalle> tipoAprobadorDict,
        long? idAreaPlantilla = null,
        long? idAreaPadrePlantilla = null)
    {

        var plantilla = Plantilla.Crear(
                idPlantilla,
                request.NombrePlantilla,
                request.Descripcion,
                request.Visibilidad,
                usuarioActual,
                gerenciaUsuario,
                request.EsReutilizable,
                idAreaPlantilla,
                idAreaPadrePlantilla
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
