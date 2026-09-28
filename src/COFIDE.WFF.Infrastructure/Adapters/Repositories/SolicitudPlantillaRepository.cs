using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPorId;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Adapters.Repositories.Base;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
namespace COFIDE.WFF.Infrastructure.Adapters.Repositories;

public class SolicitudPlantillaRepository(
        WorkflowFirmaDbContext context
    ) : GenericRepository<SolicitudPlantilla, long>(context), ISolicitudPlantillaRepository
{
    public async Task<PlantillaEtapaDto?> ObtenerEtapaActual(
         long IdSolicitud,
         long? IdSolicitudPlantilla,
         CancellationToken cancellationToken)
    {
        var query = _context.SolicitudFirmanteFlujos
            .AsNoTracking()
            .Where(sf =>
                sf.Estado &&
                !sf.RevAprobado &&
                sf.SolicitudPlantilla.IdSolicitud == IdSolicitud &&
                sf.SolicitudPlantilla.Estado);

        if (IdSolicitudPlantilla.HasValue && IdSolicitudPlantilla.Value > 0)
        {
            query = query.Where(sf => sf.IdSolicitudPlantilla == IdSolicitudPlantilla.Value);
        }

        var ordenEtapaActual = await query
            .Select(sf => (long?)sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa)
            .MinAsync(cancellationToken);

        if (!ordenEtapaActual.HasValue)
            return null;

        return await query
            .Where(sf => sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == ordenEtapaActual.Value)
            .Select(sf => new PlantillaEtapaDto
            {
                OrdenEtapa = sf.FirmanteFlujo.PlantillaEtapa.OrdenEtapa,
                IdTipoEtapa = sf.FirmanteFlujo.PlantillaEtapa.IdTipoEtapa,
                NombreEtapa = sf.FirmanteFlujo.PlantillaEtapa.NombreEtapa.Valor,

                ValorTipoEtapa = _context.ParametroDetalles
                    .Where(x => x.Id == sf.FirmanteFlujo.PlantillaEtapa.IdTipoEtapa)
                    .Select(x => x.ValorParametro)
                    .FirstOrDefault() ?? "",

                Firmantes = query
                    .Where(f => f.FirmanteFlujo.PlantillaEtapa.OrdenEtapa == ordenEtapaActual.Value)
                    .Select(f => new FirmanteFlujoDto
                    {
                        IdPlantillaEtapa = f.IdPlantillaEtapa,
                        IdUsuarioAsignado = f.IdUsuarioAsignado,

                        NombreUsuarioAsignado =
                            f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres + " " +
                            f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno + " " +
                            (f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoMaterno ?? ""),

                        Nombres = f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Nombres,
                        ApellidoPaterno = f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoPaterno,
                        ApellidoMaterno = f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.ApellidoMaterno ?? "",
                        Correo = f.FirmanteFlujo.FirmanteFlujoUsuarioAsignado.Email ?? "",
                        CargoUsuario = f.FirmanteFlujo.CargoUsuario,
                        AreaUsuario = f.FirmanteFlujo.AreaUsuario,
                        GerenciaUsuario = f.FirmanteFlujo.GerenciaUsuario
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}