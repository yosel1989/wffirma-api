using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.DescargarReporteSolicitudExcel;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitudPaginado;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using VO = COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

/// <summary>
/// Lectura: listado paginado de solicitudes y reporte Excel (mismos filtros y misma proyección).
/// </summary>
public partial class SolicitudQueryRepository
{
    /// <summary>
    /// Proyección única del listado de solicitudes: la usan la grilla paginada y el reporte Excel.
    /// </summary>
    private static readonly Expression<Func<Solicitud, SolicitudPaginadoDto>> ProyeccionListado =
        s => new SolicitudPaginadoDto
           {
               IdSolicitud = s.Id,
               CodigoSolicitud = s.CodigoSolicitud,
               TipoSolicitud = s.TipoSolicitud.NombreTipoSolicitud,
               SubTipoSolicitud = s.SubTipoSolicitud.NombreSubTipoSolicitud,
               TipoOperacion = s.TipoOperacion.NombreTipoOperacion,
               Urgente = s.EsUrgente ? "Sí" : "No",
               Solicitante = (
                    (s.UsuarioSolicitante.Nombres.Contains(" ")
                        ? s.UsuarioSolicitante.Nombres.Substring(0, s.UsuarioSolicitante.Nombres.IndexOf(" "))
                        : s.UsuarioSolicitante.Nombres)
                    + " " +
                    (s.UsuarioSolicitante.ApellidoPaterno ?? "")
                ).Trim(),
               CodigoSolicitante = s.UsuarioSolicitante.CuentaLogueo,
               Estado = s.EstadoSolicitud.Nombre,
               FechaCreacion = s.FechaSolicitud,
               AprobadoresPendientes = s.SolicitudPlantilla.FirmantesFlujo
                            .Where(sf => sf.Estado && sf.FirmanteFlujo.Estado && !sf.RevAprobado && sf.FechaRevision == null)
                            .Select(sf => sf.Id)
                            .Distinct()
                            .LongCount(),
               AprobadorActual = (s.IdEstadoSolicitud == VO.EstadoSolicitud.Rechazado.Codigo || s.IdEstadoSolicitud == VO.EstadoSolicitud.Borrador.Codigo)
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
           };


    [SuppressMessage(
       "Major Code Smell",
       "S3776:Cognitive Complexity of methods should not be too high",
       Justification = "AplicarFiltros")]
    private static IQueryable<Solicitud> AplicarFiltrosUniverso(
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
    List<long>? aprobadorActual,
    List<long>? estado,
    string? codigosolicitud,
    string? asunto,
    string? referencia,
    bool? urgente,
    bool? usuario,
    bool? usuariodepartamento,
    List<long>? idsAreasAccesoEspecial)
    {
        // Visibilidad primero (rol OR acceso especial); los filtros siguientes solo reducen.
        query = AplicarVisibilidad(query, codigoUsuario, areaUsuario, roLUsuario, idsAreasAccesoEspecial);

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

        /* if (urgente == true)
         {
             query = query.Where(s =>
                 s.EsUrgente);
         }*/

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

        return query;
    }

    /// <summary>
    /// Qué solicitudes puede ver el usuario: lo que ya ve por su rol OR las de las áreas
    /// con acceso especial activo (una gerencia incluye sus departamentos).
    /// Reescrita en una sola lambda por rol (sin el combinador Or ni HasValue/.Value) para que
    /// EF Core + Oracle no genere literales booleanos (ORA-00904: "FALSE").
    /// </summary>
    private static IQueryable<Solicitud> AplicarVisibilidad(
        IQueryable<Solicitud> query,
        long? codigoUsuario,
        long? areaUsuario,
        string? roLUsuario,
        List<long>? idsAreasAccesoEspecial)
    {
        var rol = RolUsuarioCodigo.Desde(roLUsuario);
        if (rol is null)
            return query;

        var tieneAcceso = idsAreasAccesoEspecial is { Count: > 0 };

        if (rol == RolUsuario.Coordinador && areaUsuario.HasValue)
        {
            var area = areaUsuario.Value;

            // Sin acceso especial no cambia nada.
            if (!tieneAcceso)
                return query.Where(s => s.UsuarioSolicitante.IdArea == area);

            var ids = idsAreasAccesoEspecial!;

            // rol OR acceso especial
            return query.Where(s =>
                s.UsuarioSolicitante.IdArea == area ||
                // acceso a un departamento: coincide el área del solicitante
                ids.Contains(s.UsuarioSolicitante.IdArea!.Value) ||
                // acceso a una gerencia: coincide el padre del área del solicitante
                ids.Contains(s.UsuarioSolicitante.Area.IdAreaPadre!.Value));
        }

        if (rol == RolUsuario.SolicitanteAprobador && codigoUsuario.HasValue)
        {
            var usuario = codigoUsuario.Value;

            // Sin acceso especial no cambia nada.
            if (!tieneAcceso)
                return query.Where(s => s.UsuarioSolicitante.Id == usuario);

            var ids = idsAreasAccesoEspecial!;

            // rol OR acceso especial
            return query.Where(s =>
                s.UsuarioSolicitante.Id == usuario ||
                // acceso a un departamento
                ids.Contains(s.UsuarioSolicitante.IdArea!.Value) ||
                // acceso a una gerencia
                ids.Contains(s.UsuarioSolicitante.Area.IdAreaPadre!.Value));
        }

        // Rol sin restricción: ya ve todo, el acceso especial no agrega nada.
        return query;
    }

    private static Expression<Func<T, bool>> Or<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parametro = left.Parameters[0];
        var cuerpoDerecho = new ReemplazarParametro(right.Parameters[0], parametro).Visit(right.Body)!;

        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(left.Body, cuerpoDerecho),
            parametro);
    }

    private sealed class ReemplazarParametro : ExpressionVisitor
    {
        private readonly ParameterExpression _origen;
        private readonly Expression _destino;

        public ReemplazarParametro(ParameterExpression origen, Expression destino)
        {
            _origen = origen;
            _destino = destino;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _origen ? _destino : base.VisitParameter(node);
    }

    private static IQueryable<Solicitud> AplicarFiltrosGrilla(
    IQueryable<Solicitud> query,
    long? codigoUsuario,
    long? areaUsuario,
    string? roLUsuario,
    long? departamentoUsuario,
    bool? urgente,
    bool? usuario,
    bool? usuariodepartamento)
    {

        if (usuario == true && codigoUsuario.HasValue)
        {
            query = query.Where(s =>
                s.UsuarioSolicitante.Id == codigoUsuario.Value);
        }

        if (usuariodepartamento == true && departamentoUsuario.HasValue)
        {
            query = query.Where(s =>
                s.UsuarioSolicitante.IdArea.HasValue &&
                s.UsuarioSolicitante.IdArea.Value == departamentoUsuario.Value);
        }

        if (urgente == true)
        {
            query = query.Where(s =>
                s.EsUrgente);
        }

        return query;
    }

    private static async Task<SolicitudResumenPaginadoDto> ObtenerResumenAsync(
    IQueryable<Solicitud> query,
    long? codigoUsuario,
    long? departamentoUsuario,
    CancellationToken cancellationToken)
    {
        return await query
            .GroupBy(_ => 1)
            .Select(g => new SolicitudResumenPaginadoDto
            {
                Total = g.LongCount(),

                Urgente = g.LongCount(x => x.EsUrgente),

                MisSolicitudes = codigoUsuario.HasValue
                    ? g.LongCount(x =>
                        x.UsuarioSolicitante.Id == codigoUsuario.Value)
                    : 0,

                MiDepartamento = departamentoUsuario.HasValue
                    ? g.LongCount(x =>
                        x.UsuarioSolicitante.IdArea.HasValue &&
                        x.UsuarioSolicitante.IdArea.Value == departamentoUsuario.Value)
                    : 0
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new SolicitudResumenPaginadoDto();
    }

    [SuppressMessage(
      "Major Code Smell",
      "S3776:Cognitive Complexity of methods should not be too high",
      Justification = "ObtenerPaginado")]
    public async Task<PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>> ObtenerPaginado(
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
        List<long>? aprobadorActual,
        List<long>? estado,
        string? codigosolicitud,
        string? asunto,
        string? referencia,
        bool? urgente,
        bool? usuario,
        bool? usuariodepartamento,
        bool? incluirresumen,
        List<long>? idsAreasAccesoEspecial,
        CancellationToken cancellationToken)
    {
        SolicitudResumenPaginadoDto resumen = new();

        var queryuniverso = AplicarFiltrosUniverso(
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
            aprobadorActual,
            estado,
            codigosolicitud,
            asunto,
            referencia,
            urgente,
            usuario,
            usuariodepartamento,
            idsAreasAccesoEspecial);

        if (incluirresumen == true)
        {
            resumen = await ObtenerResumenAsync(
            queryuniverso,
            codigoUsuario,
            departamentoUsuario,
            cancellationToken);
        }

        var querygrilla = AplicarFiltrosGrilla(
            queryuniverso,
            codigoUsuario,
            areaUsuario,
            roLUsuario,
            departamentoUsuario,
            urgente,
            usuario,
            usuariodepartamento);

        var projectedQuery = querygrilla
           .OrderByDescending(s => s.EsUrgente)
           .ThenBy(s => s.FechaSolicitud)
           .Select(ProyeccionListado);

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


        var pagedItems = new PagedResult<SolicitudPaginadoDto, SolicitudResumenPaginadoDto>(
          data,
          totalCount,
          paginationParams.PageNumber,
          paginationParams.PageSize)
        {
            ExtraData = resumen
        };

        return pagedItems;
    }

    /// <summary>
    /// Reporte Excel: aplica exactamente los mismos filtros y la misma proyección que la grilla paginada.
    /// </summary>
    public async Task<List<SolicitudPaginadoDto>> ObtenerListadoSolicitud(
       ReporteSolicitudExcelRequest request, CancellationToken cancellationToken, bool tracking = false)
    {
        request.Skip = Math.Max(0, request.Skip);
        request.Take = request.Take <= 0 ? 10 : request.Take;

        IQueryable<Solicitud> query = _context.Set<Solicitud>();

        if (!tracking)
            query = query.AsNoTracking();

        query = AplicarFiltrosUniverso(
            query,
            request.CodigoUsuario,
            request.AreaUsuario,
            request.RoLUsuario,
            request.DepartamentoUsuario,
            request.Fechainicio,
            request.Fechafin,
            request.Gerencia,
            request.Departamento,
            request.Solicitante,
            request.Tiposolicitud,
            request.Subtiposolicitud,
            request.TipoOperacion,
            request.AprobadorActual,
            request.Estado,
            request.Codigosolicitud,
            request.Asunto,
            request.Referencia,
            request.Urgente,
            request.Usuario,
            request.Usuariodepartamento,
            null);

        query = AplicarFiltrosGrilla(
            query,
            request.CodigoUsuario,
            request.AreaUsuario,
            request.RoLUsuario,
            request.DepartamentoUsuario,
            request.Urgente,
            request.Usuario,
            request.Usuariodepartamento);

        return await query
            .OrderByDescending(s => s.EsUrgente)
            .ThenBy(s => s.FechaSolicitud)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(ProyeccionListado)
            .ToListAsync(cancellationToken);
    }


    /// <summary>
    /// Verifica si un usuario tiene permiso para ver una solicitud específica.
    /// </summary>
    /// <param name="idSolicitud">Identificador único de la solicitud.</param>
    /// <param name="codigoUsuario">Código del usuario que realiza la consulta.</param>
    /// <param name="areaUsuario">Identificador del área del usuario.</param>
    /// <param name="roLUsuario">Rol del usuario en formato de cadena.</param>
    /// <param name="idsAreasAccesoEspecial">Lista de identificadores de áreas con acceso especial.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Un valor booleano que indica si el usuario puede ver la solicitud.</returns>
    public async Task<bool> PuedeVerSolicitudAsync(
        long idSolicitud,
        long? codigoUsuario,
        long? areaUsuario,
        string? roLUsuario,
        List<long>? idsAreasAccesoEspecial,
        CancellationToken cancellationToken)
    {
        var output = await AplicarVisibilidad(_context.Set<Solicitud>().AsNoTracking(),
                   codigoUsuario, areaUsuario, roLUsuario, idsAreasAccesoEspecial)
                    .Where(s => s.Id == idSolicitud)
                    .CountAsync(cancellationToken) > 0;
               //.AnyAsync(s => s.Id == idSolicitud, cancellationToken);

        return output;
    }
}