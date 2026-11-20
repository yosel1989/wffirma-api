using COFIDE.WFF.Application.Common.Pageds;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;

/// <summary>Valida los filtros, el ordenamiento y los parámetros de paginación de usuarios.</summary>
public sealed class ObtenerUsuariosPaginadoValidator : AbstractValidator<ObtenerUsuariosPaginadoQuery>
{
    /// <summary>Campos de salida que se pueden usar para ordenar el listado.</summary>
    private static readonly HashSet<string> CamposOrdenables = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(UsuarioPaginadoDto.IdUsuario),
        nameof(UsuarioPaginadoDto.CuentaLogueo),
        nameof(UsuarioPaginadoDto.NombreCompleto),
        nameof(UsuarioPaginadoDto.NombreGerencia),
        nameof(UsuarioPaginadoDto.NombreDepartamento),
        nameof(UsuarioPaginadoDto.NombreCargo),
        nameof(UsuarioPaginadoDto.Estado)
    };

    /// <summary>Configura las reglas de validación de la consulta.</summary>
    public ObtenerUsuariosPaginadoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0)
            .WithMessage("El número de página debe ser mayor que cero.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, PagedDefaults.MaxPageSize)
            .WithMessage($"El tamaño de página debe estar entre 1 y {PagedDefaults.MaxPageSize}.");
        RuleFor(x => x.IdGerencia).GreaterThan(0L)
            .WithMessage("El identificador de gerencia debe ser mayor que cero.");
        RuleFor(x => x.IdDepartamento).GreaterThan(0L)
            .WithMessage("El identificador de departamento debe ser mayor que cero.");
        RuleFor(x => x.IdCargo).GreaterThan(0L)
            .WithMessage("El identificador de cargo debe ser mayor que cero.");
        RuleFor(x => x.IdUsuario).GreaterThan(0L)
            .WithMessage("El identificador de usuario debe ser mayor que cero.");
        RuleForEach(x => x.Sorts)
            .Must(sort => sort is not null && CamposOrdenables.Contains(sort.PropertyName))
            .WithMessage("El campo de ordenamiento no corresponde a una columna del listado de usuarios.");
    }
}
