using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosPaginado;
using COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class UsuarioRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<Usuario, long>(context), IUsuarioRepository
{
    public async Task<Usuario?> GetByIdAsync(long idUsuario, bool tracking = false)
    {
        IQueryable<Usuario> query = _context.Set<Usuario>();
        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.Cargo)
            .Include(u => u.Area)
                .ThenInclude(u => u.AreaPadre)
            .FirstOrDefaultAsync(u =>
                u.Id == idUsuario
                && u.Estado);
    }

    public async Task<Usuario?> GetByCuentaLogueoAsync(string cuentaLogueo, bool tracking = false)
    {
        IQueryable<Usuario> query = _context.Set<Usuario>();
        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Include(u => u.Cargo)
            .Include(u => u.Area)
                .ThenInclude(u => u.AreaPadre)
            .FirstOrDefaultAsync(u =>
                u.CuentaLogueo.ToUpper() == cuentaLogueo.ToUpper()
                && u.Estado);
    }

    /// <inheritdoc />
    public async Task<PagedResult<UsuarioPaginadoDto>> ObtenerPaginadoAsync(
        PagedQueryParams pagination, IReadOnlyCollection<long>? idGerencia, IReadOnlyCollection<long>? idDepartamento, long? idCargo,
        bool? estado, long? idUsuario, CancellationToken cancellationToken)
    {
        var departamentos = idDepartamento?.Distinct().ToList() ?? [];
        var gerencias = idGerencia?.Distinct().ToList() ?? [];
        var filtrarPorDepartamento = departamentos.Count > 0;
        var filtrarPorGerencia = !filtrarPorDepartamento && gerencias.Count > 0;

        var usuarios = _context.Usuarios.AsNoTracking();

        if (idUsuario.HasValue)
            usuarios = usuarios.Where(u => u.Id == idUsuario.Value);
        if (idCargo.HasValue)
            usuarios = usuarios.Where(u => u.IdCargo == idCargo.Value);
        if (estado.HasValue)
            usuarios = usuarios.Where(u => u.Estado == estado.Value);

        var listado = from usuario in usuarios
                      let area = usuario.Area
                      let padre = area.AreaPadre
                      let abuelo = padre.AreaPadre
                      let gerenciaId = area != null && area.Tipo == 'G' ? (long?)area.Id
                          : padre != null && padre.Tipo == 'G' ? padre.Id
                          : abuelo != null && abuelo.Tipo == 'G' ? abuelo.Id : null
                      let departamentoId = area != null && area.Tipo == 'D' ? (long?)area.Id
                          : padre != null && padre.Tipo == 'D' ? padre.Id
                          : abuelo != null && abuelo.Tipo == 'D' ? abuelo.Id : null
                      where (!filtrarPorGerencia || (gerenciaId.HasValue && gerencias.Contains(gerenciaId.Value)))
                          && (!filtrarPorDepartamento || (departamentoId.HasValue && departamentos.Contains(departamentoId.Value)))
                      select new UsuarioPaginadoDto
                      {
                          IdUsuario = usuario.Id,
                          CuentaLogueo = usuario.CuentaLogueo,
                          NombreCompleto = usuario.Nombres + " " + usuario.ApellidoPaterno
                              + (usuario.ApellidoMaterno == null ? "" : " " + usuario.ApellidoMaterno),
                          NombreGerencia = area != null && area.Tipo == 'G' ? area.NombreArea
                              : padre != null && padre.Tipo == 'G' ? padre.NombreArea
                              : abuelo != null && abuelo.Tipo == 'G' ? abuelo.NombreArea : null,
                          NombreDepartamento = area != null && area.Tipo == 'D' ? area.NombreArea
                              : padre != null && padre.Tipo == 'D' ? padre.NombreArea
                              : abuelo != null && abuelo.Tipo == 'D' ? abuelo.NombreArea : null,
                          NombreCargo = usuario.Cargo != null ? usuario.Cargo.NombreCargo : null,
                          Estado = usuario.Estado
                      };

        var total = await listado.CountAsync(cancellationToken);
        var offset = (pagination.PageNumber - 1L) * pagination.PageSize;
        if (offset >= total)
            return new PagedResult<UsuarioPaginadoDto>([], total, pagination.PageNumber, pagination.PageSize);

        var criterios = pagination.Sorts ?? [];
        var ordenado = criterios.Any(s => string.Equals(s.PropertyName,
            nameof(UsuarioPaginadoDto.IdUsuario), StringComparison.OrdinalIgnoreCase))
            ? listado.OrderByMultiple(criterios)
            : listado.OrderByMultiple(criterios.Append((nameof(UsuarioPaginadoDto.IdUsuario), false)));

        var items = await ordenado.Skip((int)offset).Take(pagination.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<UsuarioPaginadoDto>(items, total, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<List<UsuarioBuscadorDto>> BuscarAsync(string termino, CancellationToken cancellationToken)
    {
        var terminoUp = termino.Trim().ToUpper();
        var tokens = terminoUp.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var query = _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Cargo)
            .Include(u => u.Area)
                .ThenInclude(a => a.AreaPadre)
                    .ThenInclude(a => a.AreaPadre)
            .Where(u => u.Estado && u.Cargo.Estado && u.Area.Estado);

        if (tokens.Length == 1)
        {
            query = query.Where(FiltrarPorToken(tokens[0]));
        }
        else
        {
            foreach (var token in tokens)
            {
                query = query.Where(FiltrarPorToken(token));
            }
        }

        var usuarios = await query
            .OrderBy(u => u.ApellidoPaterno)
            .ThenBy(u => u.ApellidoMaterno)
            .ThenBy(u => u.Nombres)
            .Select(u => new UsuarioBuscadorDto
            {
                IdUsuario = u.Id,
                Nombres = u.Nombres,
                ApellidoPaterno = u.ApellidoPaterno,
                ApellidoMaterno = u.ApellidoMaterno ?? "",
                NombreCompleto = u.Nombres + " " + u.ApellidoPaterno + " " + (u.ApellidoMaterno ?? ""),
                Cargo = u.Cargo.NombreCargo,
                Area = u.Area.NombreArea,
                Departamento = u.Area.AreaPadre != null
                    ? u.Area.AreaPadre.NombreArea : "",
                Gerencia = u.Area.AreaPadre != null && u.Area.AreaPadre.AreaPadre != null
                    ? u.Area.AreaPadre.AreaPadre.NombreArea : ""
            })
            .ToListAsync(cancellationToken);

        return usuarios;
    }

    private static Expression<Func<Usuario, bool>> FiltrarPorToken(string t)
    {
        return u =>
            u.Nombres.ToUpper().StartsWith(t) ||
            u.Nombres.ToUpper().Contains(" " + t) ||
            u.ApellidoPaterno.ToUpper().StartsWith(t) ||
            u.ApellidoPaterno.ToUpper().Contains(" " + t) ||
            (u.ApellidoMaterno != null &&
                (u.ApellidoMaterno.ToUpper().StartsWith(t) ||
                 u.ApellidoMaterno.ToUpper().Contains(" " + t))) ||
            u.Cargo.NombreCargo.ToUpper().StartsWith(t) ||
            u.Cargo.NombreCargo.ToUpper().Contains(" " + t) ||
            u.Area.NombreArea.ToUpper().StartsWith(t) ||
            u.Area.NombreArea.ToUpper().Contains(" " + t) ||
            (u.Area.AreaPadre != null &&
                (u.Area.AreaPadre.NombreArea.ToUpper().StartsWith(t) ||
                 u.Area.AreaPadre.NombreArea.ToUpper().Contains(" " + t))) ||
            (u.Area.AreaPadre != null && u.Area.AreaPadre.AreaPadre != null &&
                (u.Area.AreaPadre.AreaPadre.NombreArea.ToUpper().StartsWith(t) ||
                 u.Area.AreaPadre.AreaPadre.NombreArea.ToUpper().Contains(" " + t)));
    }

    public async Task<List<UsuariosComboDto>> GetUsuarioCombo(bool? todos, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Usuarios.AsNoTracking();

            if (todos is null)
                query = query.Where(u => u.Estado);

            var usuarios = await query
                .OrderBy(u => u.ApellidoPaterno)
                .ThenBy(u => u.ApellidoMaterno)
                .ThenBy(u => u.Nombres)
                .Select(u => new UsuariosComboDto
                {
                    IdUsuario = u.Id,
                    NombreCompleto =
                        u.Nombres + " " +
                        u.ApellidoPaterno + " " +
                        (u.ApellidoMaterno ?? ""),
                    CodigoUsuario = u.CuentaLogueo ?? ""
                })
                .ToListAsync(cancellationToken);

            return usuarios;
        }
        catch (Exception e)
        {

            throw;
        }
 
    }
}