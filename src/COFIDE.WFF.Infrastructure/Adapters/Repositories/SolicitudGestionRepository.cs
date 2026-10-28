using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerGestionSolicitudes;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using VO = COFIDE.WFF.Domain.ValueObjects;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories
{
    public partial class SolicitudGestionRepository(WorkflowFirmaDbContext context) : ISolicitudGestionRepository
    {
        private readonly WorkflowFirmaDbContext _context = context;
        private static IQueryable<ObtenerGestionSolicitudesDto> ProyectarListadoGestion(IQueryable<Solicitud> query)
        {
            return query
                .Select(s => new
                {
                    s.Id,
                    s.CodigoSolicitud,
                    Solicitante = (
                                (s.UsuarioSolicitante.Nombres.Contains(" ")
                                    ? s.UsuarioSolicitante.Nombres.Substring(
                                        0, s.UsuarioSolicitante.Nombres.IndexOf(" "))
                                    : s.UsuarioSolicitante.Nombres)
                                + " " + (s.UsuarioSolicitante.ApellidoPaterno ?? "")
                            ).Trim(),
                    Estado = s.EstadoSolicitud.Nombre,
                    FechaCreacion = s.FechaSolicitud,
                    s.FechaModificacion,
                    IdEstado = s.IdEstadoSolicitud,
                    CicloActual = s.SolicitudPlantilla.FirmantesFlujo
                        .SelectMany(f => f.HistRevision.Where(h => h.Estado))
                        .Max(h => (int?)h.Ciclo),
                    AprobadorActual = s.IdEstadoSolicitud == VO.EstadoSolicitud.Rechazado.Codigo
                        ? "-"
                        : s.Documentos.Any()
                            ? s.Documentos
                                .SelectMany(d => d.DetallesFirma.Where(dd => dd.Estado))
                                .Where(df => df.SolicitudFirmanteFlujo.FirmanteFlujo.Estado && !df.SolicitudFirmanteFlujo.RevAprobado && df.SolicitudFirmanteFlujo.FechaRevision == null)
                                .OrderBy(df => df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
                                .Select(df => new
                                {
                                    df.IdSolicitudFirmanteFlujo,
                                    NombreCompleto = (
                                        (df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                                            ? df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                                                0, df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                                            : df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                                        + " " +
                                        (df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                                    ).Trim()
                                })
                                .Distinct()
                                .Select(x => x.NombreCompleto)
                                .FirstOrDefault()
                            : s.SolicitudPlantilla.FirmantesFlujo
                                .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado && !sf.RevAprobado && sf.FechaRevision == null)
                                .OrderBy(sf => sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
                                .Select(sf => (
                                    (sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                                        ? sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                                            0, sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                                        : sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                                    + " " +
                                    (sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                                ).Trim())
                                .FirstOrDefault()
                })
                .Select(x => new ObtenerGestionSolicitudesDto
                {
                    IdSolicitud = x.Id,
                    CodigoSolicitud = x.CodigoSolicitud,
                    Solicitante = x.Solicitante,
                    Estado = x.Estado,
                    FechaCreacion = x.FechaCreacion,
                    FechaModificacion = x.FechaModificacion,
                    AprobadorActual = x.AprobadorActual
                });
        }

        private static IQueryable<Solicitud> AplicarFiltrosPrincipalesGestion(
            IQueryable<Solicitud> query,
            long? codigoUsuario,
            long? areaUsuario,
            string? roLUsuario,
            long? departamentoUsuario,
            string? fechainicio,
            string? fechafin,
            List<long>? gerencia,
            List<long>? departamento,
            List<long>? solicitante,
            List<long>? aprobadorActual,
            List<long>? estado,
            string? codigosolicitud
        )
        {
            // Solo si se quiere incluir estados en el query
            var EstadosExcluidos = new[] {
                VO.EstadoSolicitud.EnCurso.Codigo,
                VO.EstadoSolicitud.Aprobado.Codigo,
                VO.EstadoSolicitud.Rechazado.Codigo,
            };

            query = query.Where(s => EstadosExcluidos.Contains(s.IdEstadoSolicitud));

            if (!string.IsNullOrWhiteSpace(roLUsuario))
            {
                var rol = roLUsuario.Trim().ToUpper();

                if (rol == "COORD" && areaUsuario.HasValue)
                {
                    query = query.Where(s =>
                        s.UsuarioSolicitante.IdArea.HasValue &&
                        s.UsuarioSolicitante.IdArea.Value == areaUsuario.Value);
                }
                else if (rol == "SOLAPR" && codigoUsuario.HasValue)
                {

                    query = query.Where(s => s.SolicitudPlantilla.FirmantesFlujo.Any(sf => sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value));

                }
            }

            if (DateTime.TryParse(fechainicio, out var fechaInicioParseada))
            {
                query = query.Where(s =>
                    s.FechaSolicitud >= fechaInicioParseada.Date);
            }

            if (DateTime.TryParse(fechafin, out var fechaFinParseada))
            {
                var fechaFin = fechaFinParseada.Date.AddDays(1).AddTicks(-1);

                query = query.Where(s =>
                    s.FechaSolicitud <= fechaFin);
            }

            if (gerencia?.Count > 0)
            {
                query = query.Where(s =>
                    s.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
                    gerencia.Contains(s.UsuarioSolicitante.Area.IdAreaPadre.Value));
            }

            if (departamento?.Count > 0)
            {
                query = query.Where(s =>
                    s.UsuarioSolicitante.IdArea.HasValue &&
                    departamento.Contains(s.UsuarioSolicitante.IdArea.Value));
            }

            if (solicitante?.Count > 0)
            {
                query = query.Where(s =>
                    solicitante.Contains(s.UsuarioSolicitante.Id));
            }

            if (estado?.Count > 0)
            {
                query = query.Where(s =>
                    estado.Contains(s.IdEstadoSolicitud));
            }

            if (aprobadorActual?.Count > 0)
            {
                query = query.Where(s =>
                    s.Documentos
                        .SelectMany(d => d.DetallesFirma.Where(df => df.Estado))
                        .Any(df =>
                            df.EstadoDocumento == VO.EstadoDocumento.Pendiente &&
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.Estado &&
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado != null &&
                            aprobadorActual.Contains(
                                df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id)));
            }

            if (!string.IsNullOrWhiteSpace(codigosolicitud))
            {
                var codigo = codigosolicitud.Trim();

                query = query.Where(s =>
                    s.CodigoSolicitud != null &&
                    s.CodigoSolicitud.Contains(codigo));
            }

            return query;
        }

        private static async Task<GestionSolicitudesResumenDto> ObtenerResumenSeguimientoAsync(
            IQueryable<Solicitud> query,
            CancellationToken cancellationToken
        )
        {
            return await query
                .GroupBy(_ => 1)
                .Select(g => new GestionSolicitudesResumenDto
                {
                    Total = g.LongCount()
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? new GestionSolicitudesResumenDto();
        }

        public async Task<PagedResult<ObtenerGestionSolicitudesDto, GestionSolicitudesResumenDto>> ObtenerPaginadoGestionSeguimiento(
            PagedQueryParams paginationParams,
            long? codigoUsuario,
            long? areaUsuario,
            string? roLUsuario,
            long? departamentoUsuario,
            string? fechainicio,
            string? fechafin,
            List<long>? gerencia,
            List<long>? departamento,
            List<long>? solicitante,
            List<long>? aprobadorActual,
            List<long>? estado,
            string? codigosolicitud,
            CancellationToken cancellationToken
        )
        {
            GestionSolicitudesResumenDto resumen = new();

            var queryuniverso = AplicarFiltrosPrincipalesGestion(
                _context.Set<Solicitud>().AsNoTracking(),
                codigoUsuario,
                areaUsuario,
                roLUsuario,
                departamentoUsuario,
                fechainicio,
                fechafin,
                gerencia,
                departamento,
                solicitante,
                estado,
                aprobadorActual,
                codigosolicitud
            );

            resumen = await ObtenerResumenSeguimientoAsync(
                queryuniverso,
                cancellationToken);

            var projectedQuery = ProyectarListadoGestion(queryuniverso
                                                                    .OrderByDescending(s => s.EsUrgente)
                                                                    .ThenByDescending(s => s.FechaSolicitud));

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

            var pagedItems = new PagedResult<ObtenerGestionSolicitudesDto, GestionSolicitudesResumenDto>(
                data,
                totalCount,
                paginationParams.PageNumber,
                paginationParams.PageSize)
                {
                    ExtraData = resumen
                };

            return pagedItems;
        }
    }
}
