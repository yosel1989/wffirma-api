using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.Plantillas.Ports.Out;
using COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Infrastructure.Extensions;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using VO = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class PlantillaGestionRepository(WorkflowFirmaDbContext context) : IPlantillaGestionRepository
{
    private readonly WorkflowFirmaDbContext _context = context;

    public async Task<PagedResult<GestionPlantillasDto>> ObtenerPaginadoGestion(
        PagedQueryParams paginationParams,
        long? codigoUsuario,
        long? departamentoUsuario,
        long? gerenciaUsuario,
        string? rolUsuario,
        string? fechainicio,
        string? fechafin,
        List<long>? gerencia,
        List<long>? departamento,
        List<long>? creador,
        List<long>? aprobador,
        string? nombrePlantilla,
        List<long>? areasAccesoEspecial,
        CancellationToken cancellationToken)
    {
        var query = _context.Plantillas.AsNoTracking()
            .Where(p =>
                p.EsReutilizable == true &&
                p.EstadoPlantilla != EstadoPlantilla.Eliminada);

        query = AplicarAlcancePorRol(query, codigoUsuario, departamentoUsuario, gerenciaUsuario, rolUsuario, areasAccesoEspecial);
        query = AplicarFiltros(
            query,
            fechainicio,
            fechafin,
            gerencia,
            departamento,
            creador,
            aprobador,
            nombrePlantilla);

        var projectedQuery = Proyectar(query.OrderByDescending(p => p.FechaCreacion));

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

        return new PagedResult<GestionPlantillasDto>(
            data.Select(ConvertirProyeccion).ToList(),
            totalCount,
            paginationParams.PageNumber,
            paginationParams.PageSize);
    }

    private IQueryable<Plantilla> AplicarAlcancePorRol(
        IQueryable<Plantilla> query,
        long? codigoUsuario,
        long? departamentoUsuario,
        long? gerenciaUsuario,
        string? rolUsuario,
        List<long>? areasAccesoEspecial)
    {
        var rol = RolUsuarioCodigo.Desde(rolUsuario);

        if (rol is null || rol == RolUsuario.Administrador)
            return query;

        if (rol == RolUsuario.Coordinador && departamentoUsuario.HasValue && gerenciaUsuario.HasValue)
            return AplicarAlcanceCoordinador(query, departamentoUsuario.Value, gerenciaUsuario.Value, areasAccesoEspecial);

        if (rol == RolUsuario.SolicitanteAprobador && codigoUsuario.HasValue)
            return query.Where(p => p.IdUsuarioPropietario == codigoUsuario.Value);

        return query;
    }

    private IQueryable<Plantilla> AplicarAlcanceCoordinador(
        IQueryable<Plantilla> query,
        long? departamentoUsuario,
        long? gerenciaUsuario,
        List<long>? areasAccesoEspecial)
    {
        if (areasAccesoEspecial?.Count > 0)
        {
            return query.Where(p =>
                                (p.IdAreaPlantilla == departamentoUsuario &&
                                 p.IdAreaPadrePlantilla == gerenciaUsuario)
                                ||
                                (
                                (p.IdAreaPlantilla.HasValue &&
                                 areasAccesoEspecial.Contains(p.IdAreaPlantilla.Value))
                                 ||
                                 (p.IdAreaPadrePlantilla.HasValue &&
                                 areasAccesoEspecial.Contains(p.IdAreaPadrePlantilla.Value))
                                 ));
        }

        return query.Where(p => p.IdAreaPlantilla == departamentoUsuario && p.IdAreaPadrePlantilla == gerenciaUsuario);
    }

    private IQueryable<Plantilla> AplicarFiltros(
        IQueryable<Plantilla> query,
        string? fechainicio,
        string? fechafin,
        List<long>? gerencia,
        List<long>? departamento,
        List<long>? creador,
        List<long>? aprobador,
        string? nombrePlantilla)
    {
        query = AplicarRangoFechas(query, fechainicio, fechafin);
        query = AplicarFiltrosDeArea(query, gerencia, departamento);
        query = AplicarFiltrosDePersona(query, creador, aprobador);

        if (!string.IsNullOrWhiteSpace(nombrePlantilla))
        {
            var texto = nombrePlantilla.Trim().ToUpper();

            query = query.Where(p =>
                p.NombrePlantilla != null &&
                EF.Functions.Like(p.NombrePlantilla.Valor.ToUpper().Trim(), $"%{texto}%"));
        }

        return query;
    }

    private static IQueryable<Plantilla> AplicarRangoFechas(
        IQueryable<Plantilla> query,
        string? fechainicio,
        string? fechafin)
    {
        if (DateTime.TryParse(fechainicio, out var fechaInicioParseada))
        {
            query = query.Where(p => p.FechaCreacion >= fechaInicioParseada.Date);
        }

        if (DateTime.TryParse(fechafin, out var fechaFinParseada))
        {
            var fechaFin = fechaFinParseada.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.FechaCreacion <= fechaFin);
        }

        return query;
    }

    private IQueryable<Plantilla> AplicarFiltrosDeArea(
        IQueryable<Plantilla> query,
        List<long>? gerencia,
        List<long>? departamento)
    {
        if (gerencia?.Count > 0)
        {
            query = query.Where(p => p.IdAreaPadrePlantilla.HasValue &&
            gerencia.Contains(p.IdAreaPadrePlantilla.Value));
        }

        if (departamento?.Count > 0)
        {
            query = query.Where(p => p.IdAreaPlantilla.HasValue &&
            departamento.Contains(p.IdAreaPlantilla.Value));
        }

        return query;
    }

    private static IQueryable<Plantilla> AplicarFiltrosDePersona(
        IQueryable<Plantilla> query,
        List<long>? creador,
        List<long>? aprobador)
    {
        if (creador?.Count > 0)
        {
            query = query.Where(p =>
                p.IdUsuarioPropietario.HasValue &&
                creador.Contains(p.IdUsuarioPropietario.Value));
        }

        if (aprobador?.Count > 0)
        {
            query = query.Where(p =>
                p.Etapas.Any(e =>
                    e.Estado &&
                    e.Firmantes.Any(f =>
                        f.Estado &&
                        aprobador.Contains(f.IdUsuarioAsignado))));
        }

        return query;
    }

    private IQueryable<PlantillaGestionProyeccion> Proyectar(IQueryable<Plantilla> query)
    {
        var codigoFirmante = VO.TipoAprobador.Firmante.Codigo;
        var codigoVisador = VO.TipoAprobador.Visador.Codigo;

        return query.Select(p => new PlantillaGestionProyeccion
        {
            IdPlantilla = p.Id,
            Activo = p.EstadoPlantilla == EstadoPlantilla.Activa ? 1 : 0,
            NombrePlantilla = p.NombrePlantilla != null ? p.NombrePlantilla.Valor : null,
            Creador = _context.Usuarios
                .Where(u => p.IdUsuarioPropietario.HasValue && u.Id == p.IdUsuarioPropietario.Value)
                .Select(u => (
                    (u.Nombres.Contains(" ")
                        ? u.Nombres.Substring(0, u.Nombres.IndexOf(" "))
                        : u.Nombres)
                    + " " + (u.ApellidoPaterno ?? "")
                ).Trim())
                .FirstOrDefault(),
            FechaCreacion = p.FechaCreacion,
            FechaModificacion = p.FechaModificacion,
            NumeroEtapas = p.Etapas.Count(e => e.Estado),
            NumeroFirmantes = p.Etapas
                .Where(e => e.Estado && e.ParametroTipoAprobador.CodigoDetalle == codigoFirmante)
                .SelectMany(e => e.Firmantes.Where(f => f.Estado))
                .Count(),
            NumeroVisadores = p.Etapas
                .Where(e => e.Estado && e.ParametroTipoAprobador.CodigoDetalle == codigoVisador)
                .SelectMany(e => e.Firmantes.Where(f => f.Estado))
                .Count()
        });
    }

    private static GestionPlantillasDto ConvertirProyeccion(PlantillaGestionProyeccion proyeccion) =>
        new()
        {
            IdPlantilla = proyeccion.IdPlantilla,
            Activo = proyeccion.Activo == 1,
            NombrePlantilla = proyeccion.NombrePlantilla,
            Creador = proyeccion.Creador,
            FechaCreacion = proyeccion.FechaCreacion,
            FechaModificacion = proyeccion.FechaModificacion,
            NumeroEtapas = proyeccion.NumeroEtapas,
            NumeroFirmantes = proyeccion.NumeroFirmantes,
            NumeroVisadores = proyeccion.NumeroVisadores
        };

    private sealed class PlantillaGestionProyeccion
    {
        public long IdPlantilla { get; init; }
        public int Activo { get; init; }
        public string? NombrePlantilla { get; init; }
        public string? Creador { get; init; }
        public DateTime FechaCreacion { get; init; }
        public DateTime? FechaModificacion { get; init; }
        public int NumeroEtapas { get; init; }
        public int NumeroFirmantes { get; init; }
        public int NumeroVisadores { get; init; }
    }
}
