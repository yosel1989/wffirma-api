using COFIDE.WFF.Api.Request.Comun;

namespace COFIDE.WFF.Api.Request.Usuarios;

/// <summary>
/// Parámetros de consulta para listar usuarios de forma paginada.
/// </summary>
public sealed class UsuarioPaginadoRequest : PaginadoRequest
{
    /// <summary>Gerencias opcionales. Se ignoran si se envía al menos un departamento.</summary>
    public List<long>? IdGerencia { get; set; }

    /// <summary>Departamentos opcionales. Si se envían, tienen prioridad sobre las gerencias.</summary>
    public List<long>? IdDepartamento { get; set; }

    /// <summary>Identificador opcional del cargo del usuario.</summary>
    public long? IdCargo { get; set; }

    /// <summary>Filtra usuarios activos con <c>true</c> o inactivos con <c>false</c>. Si se omite, incluye ambos.</summary>
    public bool? Estado { get; set; }

    /// <summary>Identificador opcional del usuario.</summary>
    public long? IdUsuario { get; set; }
}
