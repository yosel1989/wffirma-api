using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class SolicitudRestauracionFlujoQueryRepository(WorkflowFirmaDbContext context)
    : ISolicitudRestauracionFlujoQueryRepository
{
    private readonly WorkflowFirmaDbContext _context = context;

    public async Task<IReadOnlyList<PuntoRestauracionDto>> ObtenerPuntosRestauracion(
        long idSolicitud,
        CancellationToken cancellationToken)
    {
        var puntos = await _context.HistorialRevisiones
            .AsNoTracking()
            .Where(h =>
                h.Estado &&
                h.SolicitudPlantilla.Estado &&
                h.SolicitudPlantilla.IdSolicitud == idSolicitud)
            .OrderByDescending(h => h.FechaRevision)
            .ThenByDescending(h => h.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
            .Select(h => new
            {
                IdPuntoRestauracion = h.IdSolFirmanteFlujo,
                h.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres,
                h.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno,
                h.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoMaterno,
                CodigoTipoAprobador = h.FirmanteFlujo.PlantillaEtapa.ParametroTipoAprobador.CodigoDetalle,
                h.FechaRevision,
                h.RevAprobado
            })
            .ToListAsync(cancellationToken);

        return puntos
            .Select(p => new PuntoRestauracionDto
            {
                IdPuntoRestauracion = p.IdPuntoRestauracion,
                Nombre = string.Join(
                    " ",
                    new[] { p.Nombres, p.ApellidoPaterno, p.ApellidoMaterno }
                        .Where(parte => !string.IsNullOrWhiteSpace(parte))),
                Rol = ResolverRol(p.CodigoTipoAprobador),
                Fecha = p.FechaRevision?.ToString("o") ?? string.Empty,
                Aprobado = p.RevAprobado
            })
            .ToList();
    }

    private static string ResolverRol(string? codigoTipoAprobador) => codigoTipoAprobador switch
    {
        var codigo when codigo == TipoAprobador.Firmante.Codigo => "Firmante",
        var codigo when codigo == TipoAprobador.Visador.Codigo => "Visador",
        _ => string.Empty
    };
}
