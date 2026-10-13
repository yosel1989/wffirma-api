using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.SeguimientoSolicitudes;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using VO = COFIDE.WFF.Domain.ValueObjects;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories
{
    public partial class SolicitudQueryRepository
    {
        private static IQueryable<SeguimientoSolicitudesDto> ProyectarListadoSeguimiento(IQueryable<Solicitud> query)
        {
            return query
                .Select(s => new
                {
                    s.Id,
                    s.CodigoSolicitud,
                    TipoSolicitud = s.TipoSolicitud.NombreTipoSolicitud,
                    SubTipoSolicitud = s.SubTipoSolicitud.NombreSubTipoSolicitud,
                    Estado = s.EstadoSolicitud.Nombre,
                    FechaCreacion = s.FechaSolicitud,
                    Urgente = s.EsUrgente ? "Sí" : "No",
                    IdEstado = s.IdEstadoSolicitud,
                    CicloActual = s.SolicitudPlantilla.FirmantesFlujo
                        .SelectMany(f => f.HistRevision)
                        .Max(h => (int?)h.Ciclo),
                    Firmantes = s.SolicitudPlantilla.FirmantesFlujo
                        .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado)
                        .OrderBy(sf => sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
                        .Select(sf => new
                        {
                            sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa,
                            CodigoTipoEtapa = sf.FirmanteFlujo.PlantillaEtapa.ParametroTipoEtapa.CodigoDetalle,
                            Nombre = (
                                (sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                                    ? sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                                        0, sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                                    : sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                                + " " + (sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                            ).Trim(),
                            sf.RevAprobado,
                            sf.FechaRevision,
                            UltimoCiclo = sf.HistRevision
                                .OrderByDescending(h => h.Id)
                                .Select(h => (int?)h.Ciclo)
                                .FirstOrDefault(),
                            UltimoAprobado = sf.HistRevision
                                .OrderByDescending(h => h.Id)
                                .Select(h => (bool?)h.RevAprobado)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .Select(x => new SeguimientoSolicitudesDto
                {
                    IdSolicitud = x.Id,
                    CodigoSolicitud = x.CodigoSolicitud,
                    TipoSolicitud = x.TipoSolicitud,
                    SubTipoSolicitud = x.SubTipoSolicitud,
                    Estado = x.Estado,
                    FechaCreacion = x.FechaCreacion,
                    Urgente = x.Urgente,
                    Firmantes = x.Firmantes
                        .Select(f => new FirmantesDto
                        {
                            OrdenEtapa = f.OrdenEtapa,
                            CodigoTipoEtapa = f.CodigoTipoEtapa,
                            Nombre = f.Nombre,
                            Estado =
                                f.UltimoCiclo != null
                                && f.UltimoCiclo == x.CicloActual
                                && f.UltimoAprobado == true
                                && ((f.RevAprobado && f.FechaRevision != null
                                        && (x.IdEstado == VO.EstadoSolicitud.Aprobado.Codigo
                                            || x.IdEstado == VO.EstadoSolicitud.EnCurso.Codigo))
                                    || ((f.RevAprobado == false || f.FechaRevision == null)
                                        && x.IdEstado == VO.EstadoSolicitud.Rechazado.Codigo))
                                    ? "APROBADO"
                                    : f.UltimoCiclo != null
                                      && f.UltimoCiclo == x.CicloActual
                                      && f.UltimoAprobado == false
                                      && (f.RevAprobado == false || f.FechaRevision == null)
                                      && x.IdEstado == VO.EstadoSolicitud.Rechazado.Codigo
                                        ? "RECHAZADO"
                                        : "PENDIENTE"
                        })
                        .ToList()
                });
        }

        private static List<FirmantesDto> AgruparFirmantesSeguimiento(List<FirmantesDto>? firmantes)
        {
            if (firmantes is null || firmantes.Count == 0)
                return [];

            var resultado = new List<FirmantesDto>();
            var indice = 0;

            while (indice < firmantes.Count)
            {
                var actual = firmantes[indice];
                var esParalelo = actual.CodigoTipoEtapa == TipoEtapa.Paralelo.Codigo;

                if (!esParalelo)
                {
                    resultado.Add(new FirmantesDto
                    {
                        Nombre = actual.Nombre,
                        Estado = actual.Estado
                    });
                    indice++;
                    continue;
                }

                var paralelos = new List<FirmantesParaleloDto>();
                var orden = actual.OrdenEtapa;

                while (indice < firmantes.Count
                       && firmantes[indice].CodigoTipoEtapa == TipoEtapa.Paralelo.Codigo
                       && firmantes[indice].OrdenEtapa == orden)
                {
                    paralelos.Add(new FirmantesParaleloDto
                    {
                        Nombre = firmantes[indice].Nombre,
                        Estado = firmantes[indice].Estado
                    });
                    indice++;
                }

                resultado.Add(new FirmantesDto
                {
                    Paralelo = paralelos
                });
            }

            return resultado;
        }

        private static IQueryable<Solicitud> AplicarFiltrosPrincipalesSeguimiento(
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
    List<long>? tiposolicitud,
    List<long>? subtiposolicitud,
    List<long>? tipoOperacion,
    string? asunto,
    string? referencia,
    List<long>? estado,
    List<long>? firmante,
    string? codigosolicitud
    )
        {
            var EstadosExcluidos = new[] {
                VO.EstadoSolicitud.Borrador.Codigo,
                VO.EstadoSolicitud.Anulado.Codigo};

            query = query.Where(s => !EstadosExcluidos.Contains(s.IdEstadoSolicitud));

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
                    query = query.Where(s =>
                    s.Documentos
                        .SelectMany(d => d.DetallesFirma)
                        .Any(df =>
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado != null &&
                                df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value));
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

            if (tiposolicitud?.Count > 0)
            {
                query = query.Where(s =>
                    tiposolicitud.Contains(s.IdTipoSolicitud));
            }

            if (subtiposolicitud?.Count > 0)
            {
                query = query.Where(s =>
                    subtiposolicitud.Contains(s.IdSubTipoSolicitud));
            }

            if (tipoOperacion?.Count > 0)
            {
                query = query.Where(s =>
                    tipoOperacion.Contains(s.IdTipoOperacion));
            }

            if (!string.IsNullOrWhiteSpace(asunto))
            {
                var asuntoFiltro = asunto.Trim();

                query = query.Where(s =>
                    s.Asunto != null &&
                    s.Asunto.Contains(asuntoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(referencia))
            {
                var referenciaFiltro = referencia.Trim();

                query = query.Where(s =>
                    s.CodigosReferencia.Any(r =>
                        r.CodigoReferencia.Valor.Contains(referenciaFiltro)));
            }

            if (estado?.Count > 0)
            {
                query = query.Where(s =>
                    estado.Contains(s.IdEstadoSolicitud));
            }

            if (firmante?.Count > 0)
            {
                query = query.Where(s =>
                    s.Documentos
                        .SelectMany(d => d.DetallesFirma)
                        .Any(df =>
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado != null &&
                            firmante.Contains(
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

        private static async Task<SeguimientoSolicitudesResumenDto> ObtenerResumenSeguimientoAsync(
    IQueryable<Solicitud> query,
    CancellationToken cancellationToken)
        {
            return await query
                .GroupBy(_ => 1)
                .Select(g => new SeguimientoSolicitudesResumenDto
                {
                    Total = g.LongCount(),

                    EnCurso = g.LongCount(x => x.IdEstadoSolicitud == VO.EstadoSolicitud.EnCurso.Codigo),

                    Aprobado = g.LongCount(x => x.IdEstadoSolicitud == VO.EstadoSolicitud.Aprobado.Codigo),

                    Rechazado = g.LongCount(x => x.IdEstadoSolicitud == VO.EstadoSolicitud.Rechazado.Codigo),

                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? new SeguimientoSolicitudesResumenDto();
        }

        private static IQueryable<Solicitud> AplicarFiltrosGrillaSeguimiento(
    IQueryable<Solicitud> query,
    bool? encurso,
    bool? aprobada,
    bool? rechazada)
        {
            if (encurso.HasValue && encurso == true)
            {
                query = query.Where(s =>
                    s.IdEstadoSolicitud == VO.EstadoSolicitud.EnCurso.Codigo);
            }

            if (aprobada.HasValue && aprobada == true)
            {
                query = query.Where(s =>
                    s.IdEstadoSolicitud == VO.EstadoSolicitud.Aprobado.Codigo);
            }

            if (rechazada.HasValue && rechazada == true)
            {
                query = query.Where(s =>
                    s.IdEstadoSolicitud == VO.EstadoSolicitud.Rechazado.Codigo);
            }

            return query;
        }


        [SuppressMessage(
      "Major Code Smell",
      "S3776:Cognitive Complexity of methods should not be too high",
      Justification = "ObtenerPaginado")]
        public async Task<PagedResult<SeguimientoSolicitudesDto, SeguimientoSolicitudesResumenDto>> ObtenerPaginadoSeguimiento(
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
        List<long>? tiposolicitud,
        List<long>? subtiposolicitud,
        List<long>? tipoOperacion,
        string? asunto,
        string? referencia,
        List<long>? estado,
        List<long>? firmante,
        string? codigosolicitud,
        bool? encurso,
        bool? aprobada,
        bool? rechazada,
        bool? incluirresumen,
        CancellationToken cancellationToken)
        {
            SeguimientoSolicitudesResumenDto resumen = new();

            var queryuniverso = AplicarFiltrosPrincipalesSeguimiento(
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
                tiposolicitud,
                subtiposolicitud,
                tipoOperacion,
                asunto,
                referencia,
                estado,
                firmante,
                codigosolicitud);

            if (incluirresumen == true)
            {
                resumen = await ObtenerResumenSeguimientoAsync(
                queryuniverso,
                cancellationToken);
            }

            var querygrilla = AplicarFiltrosGrillaSeguimiento(
                queryuniverso,
                encurso,
                aprobada,
                rechazada);

            var projectedQuery = ProyectarListadoSeguimiento(querygrilla
                                                                    .OrderByDescending(s => s.EsUrgente)
                                                                    .ThenBy(s => s.FechaSolicitud));

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

            foreach (var solicitud in data)
                solicitud.Firmantes = AgruparFirmantesSeguimiento(solicitud.Firmantes);

            var pagedItems = new PagedResult<SeguimientoSolicitudesDto, SeguimientoSolicitudesResumenDto>(
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
