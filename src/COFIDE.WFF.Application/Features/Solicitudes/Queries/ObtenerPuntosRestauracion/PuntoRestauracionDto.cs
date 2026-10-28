namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;

public class PuntoRestauracionDto
{
    /// <summary>Identificador del punto de restauración.</summary>
    public long IdPuntoRestauracion { get; set; }

    /// <summary>Nombre de la persona que intervino en ese punto del flujo.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Rol con el que intervino. Valores esperados: Firmante y Visador.</summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>Fecha y hora de la intervención, en formato ISO 8601.</summary>
    public string Fecha { get; set; } = string.Empty;

    /// <summary>True cuando CH_REV_APROBADO es S. False cuando es N (rechazo).</summary>
    public bool Aprobado { get; set; }
}
