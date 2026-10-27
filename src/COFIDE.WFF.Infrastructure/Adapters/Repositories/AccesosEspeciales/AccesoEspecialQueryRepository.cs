using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using DocumentFormat.OpenXml.Bibliography;
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
                    s.IdGerencia,
                    GerenciaNombre = s.Gerencia.NombreArea,
                    s.IdDepartamento,
                    DepartamentoNombre = s.Departamento.NombreArea,
                    s.FechaCreacion,
                    s.FechaModificacion
                })
                .Select(x => new AccesoEspecialPaginadoDto
                {
                    IdAccesoEspecial = x.Id,
                    IdUsuario = x.IdUsuario,
                    NombreUsuario = x.Usuario,
                    IdGerencia = x.IdGerencia,
                    NombreGerencia = x.GerenciaNombre,
                    IdDepartamento = x.IdDepartamento,
                    NombreDepartamento = x.DepartamentoNombre,
                    FechaCreacion = x.FechaCreacion
                });
        }

        private static IQueryable<AccesoEspecial> AplicarFiltrosPrincipales(
            IQueryable<AccesoEspecial> query,
            long? idGerencia,
            long? idDepartamento,
            long? idUsuario
        )
        {
          

            if (idDepartamento is not null)
            {
                query = query.Where(s =>
                    s.IdDepartamento.HasValue &&
                    s.IdDepartamento.Value == idDepartamento);
            }
            else
            {
                query = query.Where(s =>
                    s.IdGerencia == idGerencia);
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
            long? idDepartamento,
            long? idUsuario,
            CancellationToken cancellationToken)
        {
            AccesoEspecialResumenPaginadoDto resumen = new();

            var queryuniverso = AplicarFiltrosPrincipales(
                _context.Set<AccesoEspecial>().AsNoTracking(),
                idGerencia,
                idDepartamento,
                idUsuario
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
