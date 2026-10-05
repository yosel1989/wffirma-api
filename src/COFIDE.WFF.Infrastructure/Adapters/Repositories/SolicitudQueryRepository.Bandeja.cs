using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerBandejaEntrada;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Abstractions;
using COFIDE.WFF.Domain.Common.Pageds;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using VO = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

/// <summary>
/// Lectura: bandeja de entrada del aprobador.
/// </summary>
public partial class SolicitudQueryRepository
{

    [SuppressMessage(
  "Major Code Smell",
  "S3776:Cognitive Complexity of methods should not be too high",
  Justification = "Consulta compleja de para obtener solicitud para firma. Se mantiene temporalmente por compatibilidad con EF Core.")]
    public async Task<PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>> ObtenerBandejaEntrada(
       PagedQueryParams paginationParams,
       long? codigoUsuario,
       string? fechainicio,
       string? fechafin,
       List<long>? gerencia,
       List<long>? departamento,
       List<long>? solicitante,
       List<long>? tiposolicitud,
       List<long>? subtiposolicitud,
       List<long>? tipooperacion,
       string? codigosolicitud,
       string? asunto,
       string? referencia,
       bool? urgente,
       bool? incluirresumen,
       CancellationToken cancellationToken)
    {
        var estadoEnCurso = VO.EstadoSolicitud.EnCurso.Codigo;

        IQueryable<DetalleFirma> detalleFirmasQuery = _context.DetalleFirmas;

        IQueryable<SolicitudFirmanteFlujo> solicitudFirmanteFlujoQuery = _context.Set<SolicitudFirmanteFlujo>();

        BandejaEntradaResumenDto ResumenConDocumentos = new();
        BandejaEntradaResumenDto ResumenSinDocumentos = new();

        var etapasPendientesMinimas = solicitudFirmanteFlujoQuery
            .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado &&
                !sf.RevAprobado &&
                sf.FechaRevision == null)
            .GroupBy(sf => sf.SolicitudPlantilla.Solicitud.Id)
            .Select(g => new
            {
                IdSolicitud = g.Key,
                OrdenEtapaMinima = g.Min(x => x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
            });

        IQueryable<DetalleFirma> queryConDocumentos = detalleFirmasQuery
            .Where(df =>
                df.Estado &&
                codigoUsuario.HasValue &&
                df.SolicitudDocumento.Solicitud.IdEstadoSolicitud == estadoEnCurso &&
                df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Estado &&
                df.SolicitudFirmanteFlujo.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value &&

                etapasPendientesMinimas.Any(e =>
                    e.IdSolicitud == df.SolicitudDocumento.Solicitud.Id &&
                    e.OrdenEtapaMinima == df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa
                ) &&
                !df.SolicitudFirmanteFlujo.RevAprobado &&
                df.SolicitudFirmanteFlujo.FechaRevision == null &&
                (
                    df.EstadoDocumento == VO.EstadoDocumento.Pendiente ||
                    df.EstadoDocumento == VO.EstadoDocumento.Firmado ||
                    df.EstadoDocumento == VO.EstadoDocumento.Bloqueado
                ) &&

                (
                    (
                        df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == 1
                    )
                    ||
                    detalleFirmasQuery.Any(dfAnt =>
                        dfAnt.SolicitudDocumento.Solicitud.Id == df.SolicitudDocumento.Solicitud.Id &&
                        dfAnt.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id ==
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id &&
                        dfAnt.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa <
                            df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.OrdenEtapa &&
                        dfAnt.SolicitudFirmanteFlujo.RevAprobado &&
                        dfAnt.SolicitudFirmanteFlujo.FechaRevision != null
                    )
                )
            );

        IQueryable<SolicitudFirmanteFlujo> querySinDocumentos = solicitudFirmanteFlujoQuery
            .Where(sf =>
                sf.Estado &&
                codigoUsuario.HasValue &&
                sf.SolicitudPlantilla.Solicitud.IdEstadoSolicitud == estadoEnCurso &&
                sf.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Id == codigoUsuario.Value &&
                !sf.SolicitudPlantilla.Solicitud.Documentos.Any() &&
                !sf.RevAprobado &&
                sf.FechaRevision == null &&
                etapasPendientesMinimas.Any(e =>
                    e.IdSolicitud == sf.SolicitudPlantilla.Solicitud.Id &&
                    e.OrdenEtapaMinima == sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa
                ) &&
                (
                    (
                        sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == 1
                    )
                    ||
                    solicitudFirmanteFlujoQuery.Any(sfAnt =>
                     sfAnt.FirmanteFlujo.Estado &&
                        sfAnt.SolicitudPlantilla.Solicitud.Id == sf.SolicitudPlantilla.Solicitud.Id &&
                        sfAnt.FirmanteFlujo.PlantillaEtapa.Plantilla.Id == sf.FirmanteFlujo.PlantillaEtapa.Plantilla.Id &&
                        sfAnt.FirmanteFlujo.PlantillaEtapa.OrdenEtapa < sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa &&
                        sfAnt.RevAprobado &&
                        sfAnt.FechaRevision != null
                    )
                )
            );

        #region 1. Filtro Principales
        if (!string.IsNullOrWhiteSpace(fechainicio) &&
            DateTime.TryParse(fechainicio, out var fechaInicioValue))
        {
            var fechaInicioDate = fechaInicioValue.Date;

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.FechaCreacion >= fechaInicioDate);

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.FechaCreacion >= fechaInicioDate);
        }

        if (!string.IsNullOrWhiteSpace(fechafin) &&
            DateTime.TryParse(fechafin, out var fechaFinValue))
        {
            var fechaFinDate = fechaFinValue.Date.AddDays(1);

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.FechaCreacion < fechaFinDate);

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.FechaCreacion < fechaFinDate);
        }

        if (gerencia != null && gerencia.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
                gerencia.Contains(x.SolicitudDocumento.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.Value));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
                gerencia.Contains(x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Area.IdAreaPadre.Value));
        }

        if (departamento != null && departamento.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.UsuarioSolicitante.IdArea.HasValue &&
                departamento.Contains(x.SolicitudDocumento.Solicitud.UsuarioSolicitante.IdArea.Value));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.IdArea.HasValue &&
                departamento.Contains(x.SolicitudPlantilla.Solicitud.UsuarioSolicitante.IdArea.Value));
        }

        if (solicitante != null && solicitante.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                solicitante.Contains(x.SolicitudDocumento.Solicitud.IdUsuarioSolicitante));

            querySinDocumentos = querySinDocumentos.Where(x =>
                solicitante.Contains(x.SolicitudPlantilla.Solicitud.IdUsuarioSolicitante));
        }

        if (tiposolicitud != null && tiposolicitud.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                tiposolicitud.Contains(x.SolicitudDocumento.Solicitud.IdTipoSolicitud));

            querySinDocumentos = querySinDocumentos.Where(x =>
                tiposolicitud.Contains(x.SolicitudPlantilla.Solicitud.IdTipoSolicitud));
        }

        if (subtiposolicitud != null && subtiposolicitud.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                subtiposolicitud.Contains(x.SolicitudDocumento.Solicitud.IdSubTipoSolicitud));

            querySinDocumentos = querySinDocumentos.Where(x =>
                subtiposolicitud.Contains(x.SolicitudPlantilla.Solicitud.IdSubTipoSolicitud));
        }

        if (tipooperacion != null && tipooperacion.Count > 0)
        {
            queryConDocumentos = queryConDocumentos.Where(x =>
                tipooperacion.Contains(x.SolicitudDocumento.Solicitud.IdTipoOperacion));

            querySinDocumentos = querySinDocumentos.Where(x =>
                tipooperacion.Contains(x.SolicitudPlantilla.Solicitud.IdTipoOperacion));
        }

        if (!string.IsNullOrWhiteSpace(codigosolicitud))
        {
            var codigoSolicitudFiltro = codigosolicitud.Trim();

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.CodigoSolicitud.Contains(codigoSolicitudFiltro));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.CodigoSolicitud.Contains(codigoSolicitudFiltro));
        }

        if (!string.IsNullOrWhiteSpace(asunto))
        {
            var asuntoFiltro = asunto.Trim();

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.Asunto != null &&
                x.SolicitudDocumento.Solicitud.Asunto.Contains(asuntoFiltro));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.Asunto != null &&
                x.SolicitudPlantilla.Solicitud.Asunto.Contains(asuntoFiltro));
        }

        if (!string.IsNullOrWhiteSpace(referencia))
        {
            var referenciaFiltro = referencia.Trim();

            queryConDocumentos = queryConDocumentos.Where(x =>
                x.SolicitudDocumento.Solicitud.CodigosReferencia
                    .Any(r => r.CodigoReferencia.Valor.Contains(referenciaFiltro)));

            querySinDocumentos = querySinDocumentos.Where(x =>
                x.SolicitudPlantilla.Solicitud.CodigosReferencia
                    .Any(r => r.CodigoReferencia.Valor.Contains(referenciaFiltro)));
        }
        #endregion

        var conDocumentos = await queryConDocumentos
            .Select(df => new
            {
                IdSolicitud = df.SolicitudDocumento.Solicitud.Id,
                df.SolicitudDocumento.Solicitud.CodigoSolicitud,
                TipoSolicitud = df.SolicitudDocumento.Solicitud.TipoSolicitud.NombreTipoSolicitud,
                SubTipoSolicitud = df.SolicitudDocumento.Solicitud.SubTipoSolicitud.NombreSubTipoSolicitud,
                TipoOperacion = df.SolicitudDocumento.Solicitud.TipoOperacion.NombreTipoOperacion,
                df.SolicitudDocumento.Solicitud.EsUrgente,
                Solicitante = (
                    (df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres.Contains(" ")
                        ? df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres.Substring(
                            0,
                            df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres.IndexOf(" "))
                        : df.SolicitudDocumento.Solicitud.UsuarioSolicitante.Nombres)
                    + " " +
                    (df.SolicitudDocumento.Solicitud.UsuarioSolicitante.ApellidoPaterno ?? "")
                ).Trim(),
                df.SolicitudDocumento.Solicitud.FechaCreacion,
                FechaLimite = df.SolicitudDocumento.Solicitud.FechaLimite != null
                    ? df.SolicitudDocumento.Solicitud.FechaLimite.Valor
                    : (DateTime?)null,
                IdPlantilla = df.SolicitudFirmanteFlujo.FirmanteFlujo.PlantillaEtapa.Plantilla.Id
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var sinDocumentos = await querySinDocumentos
            .Select(sf => new
            {
                IdSolicitud = sf.SolicitudPlantilla.Solicitud.Id,
                sf.SolicitudPlantilla.Solicitud.CodigoSolicitud,
                TipoSolicitud = sf.SolicitudPlantilla.Solicitud.TipoSolicitud.NombreTipoSolicitud,
                SubTipoSolicitud = sf.SolicitudPlantilla.Solicitud.SubTipoSolicitud.NombreSubTipoSolicitud,
                TipoOperacion = sf.SolicitudPlantilla.Solicitud.TipoOperacion.NombreTipoOperacion,
                sf.SolicitudPlantilla.Solicitud.EsUrgente,
                Solicitante = (
                    (sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres.Contains(" ")
                        ? sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres.Substring(
                            0,
                            sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres.IndexOf(" "))
                        : sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.Nombres)
                    + " " +
                    (sf.SolicitudPlantilla.Solicitud.UsuarioSolicitante.ApellidoPaterno ?? "")
                ).Trim(),
                sf.SolicitudPlantilla.Solicitud.FechaCreacion,
                FechaLimite = sf.SolicitudPlantilla.Solicitud.FechaLimite != null
                    ? sf.SolicitudPlantilla.Solicitud.FechaLimite.Valor
                    : (DateTime?)null,
                IdPlantilla = sf.FirmanteFlujo.PlantillaEtapa.Plantilla.Id
            })
            .Distinct()
            .ToListAsync(cancellationToken);


        #region 2. Generar Resumen
        if (incluirresumen.HasValue && incluirresumen.Value)
        {
            ResumenConDocumentos = conDocumentos
                                        .GroupBy(_ => 1)
                                        .Select(g => new BandejaEntradaResumenDto
                                        {
                                            Total = g.LongCount(),
                                            Urgente = g.LongCount(x => x.EsUrgente == true)
                                        })
                                        .FirstOrDefault()
                                                ?? new BandejaEntradaResumenDto();

            ResumenSinDocumentos = sinDocumentos
                                        .GroupBy(_ => 1)
                                        .Select(g => new BandejaEntradaResumenDto
                                        {
                                            Total = g.LongCount(),
                                            Urgente = g.LongCount(x => x.EsUrgente == true)
                                        })
                                        .FirstOrDefault()
                                            ?? new BandejaEntradaResumenDto();
        }
        #endregion


        #region 3. Filtro Tabs(grilla)
        if (urgente.HasValue && urgente.Value)
        {
            conDocumentos = conDocumentos.Where(x => x.EsUrgente == urgente.Value).ToList();

            sinDocumentos = sinDocumentos.Where(x => x.EsUrgente == urgente.Value).ToList();
        }

        #endregion

        var baseResultado = conDocumentos
            .Concat(sinDocumentos)
            .GroupBy(x => new
            {
                x.IdSolicitud,
                x.CodigoSolicitud,
                x.TipoSolicitud,
                x.SubTipoSolicitud,
                x.TipoOperacion,
                x.EsUrgente,
                x.Solicitante,
                x.FechaCreacion,
                x.FechaLimite,
                x.IdPlantilla
            })
            .Select(g => g.First())
            .OrderByDescending(x => x.EsUrgente ? 1 : 0)
            .ThenBy(x => x.EsUrgente ? (x.FechaLimite ?? DateTime.MaxValue) : DateTime.MaxValue)
            .ThenBy(x => !x.EsUrgente ? x.FechaCreacion : DateTime.MaxValue)
            .ToList();

        //if (baseResultado.Count == 0)
        //    return [];

        var idsSolicitud = baseResultado
            .Select(x => x.IdSolicitud)
            .Distinct()
            .ToList();

        var plantillasIds = baseResultado
            .Select(x => x.IdPlantilla)
            .Distinct()
            .ToList();

        var firmantesRelacionados = await solicitudFirmanteFlujoQuery
            .Where(x =>
                x.Estado &&
                idsSolicitud.Contains(x.SolicitudPlantilla.Solicitud.Id) &&
                plantillasIds.Contains(x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id))
            .Select(x => new
            {
                IdSolicitud = x.SolicitudPlantilla.Solicitud.Id,
                IdPlantilla = x.FirmanteFlujo.PlantillaEtapa.Plantilla.Id,
                x.FirmanteFlujo.PlantillaEtapa.OrdenEtapa,
                x.RevAprobado,
                x.FechaRevision,
                Firmante = (
                    (x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Contains(" ")
                        ? x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.Substring(
                            0,
                            x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres.IndexOf(" "))
                        : x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres)
                    + " " +
                    (x.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno ?? "")
                ).Trim()
            })
            .ToListAsync(cancellationToken);

        var resultado = baseResultado
            .Select(x =>
            {
                var firmantesDeSolicitud = firmantesRelacionados
                    .Where(f => f.IdSolicitud == x.IdSolicitud && f.IdPlantilla == x.IdPlantilla)
                    .ToList();

                var ultimoFirmanteAprobado = firmantesDeSolicitud
                    .Where(f => f.RevAprobado && f.FechaRevision != null)
                    .OrderByDescending(f => f.FechaRevision)
                    .ThenByDescending(f => f.OrdenEtapa)
                    .Select(f => f.Firmante)
                    .FirstOrDefault();

                long? ultimaEtapa = firmantesDeSolicitud.Count > 0
                    ? firmantesDeSolicitud.Max(f => f.OrdenEtapa)
                    : null;

                return new BandejaEntradaDto
                {
                    IdSolicitud = x.IdSolicitud,
                    CodigoSolicitud = x.CodigoSolicitud,
                    TipoSolicitud = x.TipoSolicitud,
                    SubTipoSolicitud = x.SubTipoSolicitud,
                    TipoOperacion = x.TipoOperacion,
                    Urgente = x.EsUrgente ? "Si" : "No",
                    Solicitante = x.Solicitante,
                    FechaCreacion = x.FechaCreacion,
                    FechaLimite = x.FechaLimite,

                    AnteriorFirmante = ultimoFirmanteAprobado ?? string.Empty,

                    UltimoFirmante = ultimaEtapa.HasValue
                        ? firmantesDeSolicitud
                            .Where(f => f.OrdenEtapa == ultimaEtapa.Value)
                            .Select(f => f.Firmante)
                            .Distinct()
                            .ToList()
                        : []
                };
            }).ToList();

        var totalCount = resultado.Count;

        var data = resultado
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();


        var pagedItems = new PagedResult<BandejaEntradaDto, BandejaEntradaResumenDto>(
          data,
          totalCount,
          paginationParams.PageNumber,
          paginationParams.PageSize)
        ;

        pagedItems.ExtraData = new BandejaEntradaResumenDto
        {
            Total = ResumenConDocumentos.Total + ResumenSinDocumentos.Total,
            Urgente = ResumenConDocumentos.Urgente + ResumenSinDocumentos.Urgente
        };

        return pagedItems;
    }
}
