namespace COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargos;

/// <summary>Datos de un cargo para su selección.</summary>
public class ObtenerCargosDto
{
    /// <summary>Identificador del cargo.</summary>
    public long IdCargo { get; set; }

    /// <summary>Nombre del cargo.</summary>
    public string NombreCargo { get; set; } = null!;
}
