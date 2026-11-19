namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

/// <summary>Datos de un usuario en el listado paginado.</summary>
public sealed class UsuarioPaginadoDto
{
    /// <summary>Identificador del usuario.</summary>
    public long IdUsuario { get; init; }

    /// <summary>Cuenta de inicio de sesión, si está registrada.</summary>
    public string? CuentaLogueo { get; init; }

    /// <summary>Nombres y apellidos del usuario.</summary>
    public string NombreCompleto { get; init; } = string.Empty;

    /// <summary>Nombre de la gerencia a la que pertenece el usuario, si corresponde.</summary>
    public string? NombreGerencia { get; init; }

    /// <summary>Nombre del departamento al que pertenece el usuario, si corresponde.</summary>
    public string? NombreDepartamento { get; init; }

    /// <summary>Nombre del cargo del usuario, si está asignado.</summary>
    public string? NombreCargo { get; init; }

    /// <summary>Indica si el usuario está activo.</summary>
    public bool Estado { get; init; }
}
