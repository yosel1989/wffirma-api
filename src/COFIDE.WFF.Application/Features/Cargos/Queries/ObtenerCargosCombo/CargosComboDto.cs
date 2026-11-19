namespace COFIDE.WFF.Application.Features.Cargos.Queries.ObtenerCargosCombo;

/// <summary>Datos de un cargo para su selección en un combo.</summary>
public class CargosComboDto
{
    /// <summary>Identificador del cargo.</summary>
    public long IdCargo { get; set; }

    /// <summary>Nombre del cargo.</summary>
    public string NombreCargo { get; set; } = null!;
}
