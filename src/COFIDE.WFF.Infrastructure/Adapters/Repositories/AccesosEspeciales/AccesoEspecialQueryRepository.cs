using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories.AccesosEspeciales
{
    public partial class AccesoEspecialQueryRepository(
        WorkflowFirmaDbContext context
    ) : IAccesoEspecialQueryRepository
    {

        private readonly WorkflowFirmaDbContext _context = context;

        private static IQueryable<AccesoEspecialPaginadoDto> ProyectarListado(IQueryable<AccesoEspecial> query)
        {
            return query
                .Select(s => new
                {
                    s.Id,
                    s.IdUsuario,
                    Usuario = (
                                (s.Usuario.Nombres.Contains(" ")
                                    ? s.Usuario.Nombres.Substring(
                                        0, s.Usuario.Nombres.IndexOf(" "))
                                    : s.Usuario.Nombres)
                                + " " + (s.Usuario.ApellidoPaterno ?? "")
                            ).Trim(),
                    s.IdArea,
                    AreaNombre = s.Area.NombreArea,
                    s.Estado,
                    s.FechaCreacion,
                    s.FechaModificacion
                })
                .Select(x => new AccesoEspecialPaginadoDto
                {
                    IdAccesoEspecial = x.Id,
                    IdUsuario = x.IdUsuario,
                    NombreUsuario = x.Usuario,
                    IdArea = x.IdArea,
                    NombreArea = x.AreaNombre,
                    Estado = x.Estado,
                    FechaCreacion = x.FechaCreacion
                });
        }

        private static IQueryable<AccesoEspecial> AplicarFiltrosPrincipales(
            IQueryable<AccesoEspecial> query,
            long? idArea,
            long? idUsuario
        )
        {
          

            if (idArea is not null)
            {
                query = query.Where(s =>
                    s.IdArea == idArea);
            }

            if (idUsuario is not null)
            {
                query = query.Where(s =>
                    s.IdUsuario == idUsuario);
            }

            return query;
        }

        private static async Task<AccesoEspecialResumenPaginadoDto> ObtenerResumenAsync(
            IQueryable<AccesoEspecial> query,
            CancellationToken cancellationToken
        )
        {
            return await query
                .GroupBy(_ => 1)
                .Select(g => new AccesoEspecialResumenPaginadoDto
                {
                    Total = g.LongCount()
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? new AccesoEspecialResumenPaginadoDto();
        }


        #region PAGINADO

        public async Task<PagedResult<AccesoEspecialPaginadoDto, AccesoEspecialResumenPaginadoDto>> ObtenerPaginado(
            PagedQueryParams paginationParams,
            long? idGerencia,
            long? idArea,
            CancellationToken cancellationToken)
        {
            AccesoEspecialResumenPaginadoDto resumen = new();

            var queryuniverso = AplicarFiltrosPrincipales(
                _context.Set<AccesoEspecial>().AsNoTracking(),
                idGerencia,
                idArea
            );

            resumen = await ObtenerResumenAsync(
                queryuniverso,
                cancellationToken);

            var projectedQuery = ProyectarListado(queryuniverso
                .OrderByDescending(s => s.FechaCreacion));

            if (paginationParams.Sorts?.Count > 0)
            {
                projectedQuery = paginationParams.Sorts.Count == 1
                    ? projectedQuery.OrderByDynamic(paginationParams.Sorts[0].PropertyName, paginationParams.Sorts[0].Descending)
                    : projectedQuery.OrderByMultiple(paginationParams.Sorts);
            }

            var totalCount = await projectedQuery.CountAsync(cancellationToken);

            var data = await projectedQuery
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync(cancellationToken);

            var pagedItems = new PagedResult<AccesoEspecialPaginadoDto, AccesoEspecialResumenPaginadoDto>(
                data,
                totalCount,
                paginationParams.PageNumber,
                paginationParams.PageSize)
            {
                ExtraData = resumen
            };

            return pagedItems;
        }

        #endregion


    }
}
