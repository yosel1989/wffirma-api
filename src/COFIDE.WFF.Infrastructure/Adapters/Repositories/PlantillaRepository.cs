using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Common.Pageds;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class PlantillaRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<Plantilla, long>(context), IPlantillaRepository
{
    public async Task<Plantilla?> ObtenerAgregadoPlantillaAsync(long id, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<Plantilla> query = _context.Set<Plantilla>()
           .Include(x => x.Etapas.Where(e => e.Estado))
               .ThenInclude(x => x.Firmantes.Where(f => f.Estado));

        if (!tracking)
            query = query.AsNoTracking();

        return await query
            .Where(x => x.Estado && x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<long> ObtenerSiguienteIdAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.Plantillas
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdEtapaAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.PlantillaEtapas
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<long> ObtenerSiguienteIdFirmanteAsync(CancellationToken cancellationToken)
    {
        var maxId = await _context.FirmanteFlujos
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        return maxId + 1;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdEtapaAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.PlantillaEtapas
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>();

        if (cantidad <= 0)
            return result;

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    public async Task<List<long>> ObtenerBloqueSiguienteIdFirmanteAsync(long cantidad, CancellationToken cancellationToken)
    {
        var maxId = await _context.FirmanteFlujos
        .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0;

        var result = new List<long>();

        if (cantidad <= 0)
            return result;

        for (var i = 1; i <= cantidad; i++)
        {
            result.Add(maxId + i);
        }

        return result;
    }

    #region Guardar Flujo Plantilla
    public void GuardarFlujoPlantillaAsync(Plantilla? plantilla)
    {
        if (plantilla == null)
            return;

        if (plantilla.EsNuevo)
        {
            AgregarNuevaPlantilla(plantilla);
        }
        else
        {
            ActualizarPlantillaExistente(plantilla);
        }
    }

    private void AgregarNuevaPlantilla(Plantilla plantilla)
    {
        _context.Plantillas.Add(plantilla);
    }

    private void ActualizarPlantillaExistente(Plantilla plantilla)
    {
        _context.Attach(plantilla);
        _context.Entry(plantilla).State = EntityState.Modified;

        MarcarOwnedComoModificado(plantilla, x => x.NombrePlantilla);
        MarcarOwnedComoModificado(plantilla, x => x.Descripcion);

        foreach (var etapa in plantilla.Etapas)
        {
            ConfigurarEtapa(etapa);
        }
    }

    private void ConfigurarEtapa(PlantillaEtapa etapa)
    {
        var estado = ObtenerEstado(etapa.EsNuevo);

        _context.Entry(etapa).State = estado;
        MarcarOwned(etapa, x => x.NombreEtapa, estado);

        foreach (var firmante in etapa.Firmantes)
        {
            _context.Entry(firmante).State = ObtenerEstado(firmante.EsNuevo);
        }
    }

    private static EntityState ObtenerEstado(bool esNuevo)
    {
        return esNuevo ? EntityState.Added : EntityState.Modified;
    }

    private void MarcarOwnedComoModificado<TEntity, TOwned>(
        TEntity entity,
        Expression<Func<TEntity, TOwned?>> navigation)
        where TEntity : class
        where TOwned : class
    {
        MarcarOwned(entity, navigation, EntityState.Modified);
    }

    private void MarcarOwned<TEntity, TOwned>(
        TEntity entity,
        Expression<Func<TEntity, TOwned?>> navigation,
        EntityState state)
        where TEntity : class
        where TOwned : class
    {
        var owned = _context.Entry(entity)
            .Reference(navigation)
            .TargetEntry;

        if (owned != null)
        {
            owned.State = state;
        }
    }

    #endregion

    public async Task<Plantilla?> ValidarExistenciaNombrePlantilla(string nombrePlantilla, bool visibilidad, long usuarioId)
    {
        nombrePlantilla = nombrePlantilla.Trim().ToUpper();

        var plantilla = await _context.Plantillas
            .AsNoTracking()
            .Where(p =>
                p.Estado &&
                p.NombrePlantilla!.Valor.ToUpper() == nombrePlantilla &&
                p.Visibilidad == visibilidad &&
                (
                    visibilidad ||
                    p.IdUsuarioPropietario == usuarioId
                ))
            .FirstOrDefaultAsync();

        return plantilla;
    }


    #region Queries

    public async Task<PlantillaDto?> ObtenerDetallePlantillaPorId(long IdPlantilla)
    {
        var query = await _context.Plantillas
        .AsNoTracking()
        .Where(p => p.Estado && p.Id == IdPlantilla)
        .Select(p => new PlantillaDto
        {
            IdPlantilla = p.Id,
            Descripcion = p.Descripcion == null ? "" : p.Descripcion.Valor,
            NombrePlantilla = p.NombrePlantilla == null ? "" : p.NombrePlantilla.Valor,
            Visibilidad = p.Visibilidad,
            Etapas = p.Etapas.Where(x => x.Estado)
                .OrderBy(e => e.OrdenEtapa)
                .Select(e => new PlantillaEtapaDto
                {
                    Id = e.Id,
                    OrdenEtapa = e.OrdenEtapa,
                    NombreEtapa = e.NombreEtapa.Valor,
                    IdTipoEtapa = e.IdTipoEtapa,
                    ValorTipoEtapa = _context.ParametroDetalles
                        .Where(x => x.Id == e.IdTipoEtapa)
                        .Select(x => x.ValorParametro)
                        .FirstOrDefault() ?? "",
                    IdTipoAprobador = e.IdTipoAprobador,
                    ValorTipoAprobador = _context.ParametroDetalles
                        .Where(x => x.Id == e.IdTipoAprobador)
                        .Select(x => x.ValorParametro)
                        .FirstOrDefault() ?? "",
                    Firmantes = e.Firmantes.Where(x => x.Estado)
                         .Select(f => new FirmanteFlujoDto
                         {
                             Id = f.Id,
                             IdPlantillaEtapa = f.IdPlantillaEtapa,
                             IdUsuarioAsignado = f.IdUsuarioAsignado,
                             NombreUsuarioAsignado = f.FirmanteFlujoUsuarioAsignado != null
                                ? f.FirmanteFlujoUsuarioAsignado.Nombres + " " +
                                  f.FirmanteFlujoUsuarioAsignado.ApellidoPaterno + " " +
                                  (f.FirmanteFlujoUsuarioAsignado.ApellidoMaterno ?? "")
                                : "",
                             Nombres = f.FirmanteFlujoUsuarioAsignado!.Nombres,
                             ApellidoPaterno = f.FirmanteFlujoUsuarioAsignado.ApellidoPaterno,
                             ApellidoMaterno = f.FirmanteFlujoUsuarioAsignado.ApellidoMaterno ?? "",
                             CargoUsuario = f.CargoUsuario,
                             AreaUsuario = f.AreaUsuario,
                             GerenciaUsuario = f.GerenciaUsuario
                         })
                        .ToList()

                })
                .ToList()
        }).FirstOrDefaultAsync();

        return query;
    }

    public async Task<PagedResult<PlantillaPaginadoDto>> ObtenerPaginado(PlantillaPaginadoRequest requestPaginado)
    {

        var query = _context.Plantillas.AsNoTracking().Where(p => p.Estado && p.Visibilidad == requestPaginado.Visibilidad && p.EsReutilizable == true);

        query = AplicarPermisos(
            query,
            requestPaginado.RolUsuario,
            requestPaginado.Visibilidad,
            requestPaginado.AreaParaCoordinador,
            requestPaginado.IdUsuarioPropietario);


        if (!string.IsNullOrWhiteSpace(requestPaginado.Busqueda))
        {
            var texto = requestPaginado.Busqueda.ToUpper().Trim();

            query = query.Where(p =>
                EF.Functions.Like(p.NombrePlantilla!.Valor.ToUpper().Trim(), $"%{texto}%") ||
                EF.Functions.Like(p.Descripcion!.Valor.ToUpper().Trim(), $"%{texto}%"));
        }

        if (requestPaginado.Gerencias?.Count > 0)
        {
            query = query.Where(p => requestPaginado.Gerencias.Contains(p.GerenciaUsuario!));
        }

        if (requestPaginado.Aprobadores?.Count > 0)
        {
            query = query.Where(p =>
                p.Etapas.Any(e =>
                    e.Firmantes.Any(f =>
                        requestPaginado.Aprobadores.Contains(f.IdUsuarioAsignado))));
        }

        var projectedQuery = query.Select(p => new
        {
            IdPlantilla = p.Id,
            Descripcion = p.Descripcion!.Valor,
            NombrePlantilla = p.NombrePlantilla!.Valor,
            FechaCreado = p.FechaCreacion,
            FechaModificado = p.FechaModificacion,
            p.GerenciaUsuario,
            CantidadFirmantes = p.Etapas.Where(x => x.Estado)
                .SelectMany(e => e.Firmantes.Where(x => x.Estado)
                    .Where(f => _context.ParametroDetalles
                        .Where(x => x.Id == e.IdTipoAprobador)
                        .Select(x => x.CodigoDetalle)
                        .FirstOrDefault() == "TI_APROBADOR_FIRMANTE"))
                .Count(),
            CantidadVisadores = p.Etapas.Where(x => x.Estado)
                .SelectMany(e => e.Firmantes.Where(x => x.Estado)
                    .Where(f => _context.ParametroDetalles
                        .Where(x => x.Id == e.IdTipoAprobador)
                        .Select(x => x.CodigoDetalle)
                        .FirstOrDefault() == "TI_APROBADOR_VISADOR"))
                .Count()
        });

        if (requestPaginado.PaginationParams.Sorts?.Count > 0)
        {
            projectedQuery = requestPaginado.PaginationParams.Sorts.Count == 1
                ? projectedQuery.OrderByDynamic(requestPaginado.PaginationParams.Sorts[0].PropertyName, requestPaginado.PaginationParams.Sorts[0].Descending)
                : projectedQuery.OrderByMultiple(requestPaginado.PaginationParams.Sorts);
        }

        var totalCount = await projectedQuery.CountAsync();

        var data = await projectedQuery
            .Skip((requestPaginado.PaginationParams.PageNumber - 1) * requestPaginado.PaginationParams.PageSize)
            .Take(requestPaginado.PaginationParams.PageSize)
            .ToListAsync();


        var items = data.Select(p => new PlantillaPaginadoDto
        {
            IdPlantilla = p.IdPlantilla,
            Descripcion = string.IsNullOrWhiteSpace(p.Descripcion) ? "-" : p.Descripcion,
            NombrePlantilla = string.IsNullOrWhiteSpace(p.NombrePlantilla) ? "-" : p.NombrePlantilla,
            FechaCreado = p.FechaCreado.ToString("dd/MM/yyyy"),
            FechaModificado = p.FechaModificado?.ToString("dd/MM/yyyy") ?? "-",
            CantidadFirmantes = p.CantidadFirmantes,
            CantidadVisadores = p.CantidadVisadores,
            GerenciaUsuario = p.GerenciaUsuario
        }).ToList();


        return new PagedResult<PlantillaPaginadoDto>(items, totalCount);
    }

    private static IQueryable<Plantilla> AplicarPermisos(
       IQueryable<Plantilla> query,
       RolUsuario rolUsuario,
       bool visibilidad,
       string area,
       long idUsuario)
    {
        query = query.Where(p => p.Visibilidad == visibilidad);

        if (rolUsuario == RolUsuario.Administrador)
            return query;

        if (rolUsuario == RolUsuario.Coordinador)
        {
            if (!visibilidad)
                query = query.Where(p => p.GerenciaUsuario == area);

            return query;
        }

        if (rolUsuario == RolUsuario.SolicitanteAprobador)
        {
            if (!visibilidad)
                query = query.Where(p => p.IdUsuarioPropietario == idUsuario);

            return query;
        }

        return query;
    }

    #endregion

    #region Etapas

    public async Task<List<PlantillaEtapa>> ObtenerPlantillaEtapaPorIdPlantilla(long idPlantilla, CancellationToken cancellationToken, bool tracking = false)
    {
        IQueryable<PlantillaEtapa> query = _context.Set<PlantillaEtapa>();

        if (!tracking)
            query = query.AsNoTracking();

        query = query
            .AsSplitQuery()
            .Include(d => d.Firmantes)
            .Where(d => d.Estado && d.IdPlantilla == idPlantilla);

        return await query.ToListAsync(cancellationToken);
    }

    #endregion
}