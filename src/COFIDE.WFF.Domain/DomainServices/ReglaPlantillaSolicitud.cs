namespace COFIDE.WFF.Domain.DomainServices;

/// <summary>Qué hacer con la plantilla (flujo de aprobación) al crear o reusar una solicitud.</summary>
public enum AccionPlantillaSolicitud
{
    /// <summary>La solicitud no tiene plantilla ni flujo configurado.</summary>
    SinPlantilla,

    /// <summary>Crear una plantilla propia (no maestra) a partir del flujo configurado en la solicitud.</summary>
    CrearDesdeConfiguracion,

    /// <summary>Crear una plantilla propia copiando la plantilla maestra seleccionada.</summary>
    CrearDesdeMaestra,

    /// <summary>La plantilla seleccionada no es la de origen: aplicar el flujo sobre la plantilla de origen.</summary>
    ActualizarPlantillaOrigen,

    /// <summary>Aplicar el flujo configurado sobre la plantilla seleccionada.</summary>
    ActualizarPlantillaSeleccionada
}

/// <summary>
/// Regla de negocio para decidir cómo se obtiene la plantilla de una solicitud.
/// Las plantillas maestras nunca se modifican: si se usa una, se crea una copia propia de la solicitud.
/// </summary>
public static class ReglaPlantillaSolicitud
{
    /// <param name="idPlantillaOrigen">Plantilla con la que ya contaba la solicitud (0 si no tenía).</param>
    /// <param name="idPlantillaSeleccionada">Plantilla elegida por el usuario (null o 0 si no eligió).</param>
    /// <param name="seleccionadaEsMaestra">Si la plantilla elegida es una plantilla maestra.</param>
    /// <param name="tieneFlujoConfigurado">Si el usuario armó o modificó el flujo en la solicitud.</param>
    public static AccionPlantillaSolicitud Decidir(
        long? idPlantillaOrigen,
        long? idPlantillaSeleccionada,
        bool seleccionadaEsMaestra,
        bool tieneFlujoConfigurado)
    {
        var sinSeleccion = idPlantillaSeleccionada is null or 0;
        var maestraSinOrigenConFlujo = idPlantillaOrigen == 0 && seleccionadaEsMaestra && tieneFlujoConfigurado;

        if (sinSeleccion || maestraSinOrigenConFlujo)
            return tieneFlujoConfigurado ? AccionPlantillaSolicitud.CrearDesdeConfiguracion : AccionPlantillaSolicitud.SinPlantilla;

        if (seleccionadaEsMaestra && idPlantillaOrigen == 0)
            return AccionPlantillaSolicitud.CrearDesdeMaestra;

        if (idPlantillaOrigen != idPlantillaSeleccionada)
            return AccionPlantillaSolicitud.ActualizarPlantillaOrigen;

        return AccionPlantillaSolicitud.ActualizarPlantillaSeleccionada;
    }
}
