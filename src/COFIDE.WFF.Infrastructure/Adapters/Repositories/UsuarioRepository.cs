using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;
using COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class UsuarioRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<Usuario, long>(context), IUsuarioRepository
{
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

    public async Task<List<UsuariosComboDto>> GetUsuarioCombo(CancellationToken cancellationToken)
    {
        try
        {
var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Estado)
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