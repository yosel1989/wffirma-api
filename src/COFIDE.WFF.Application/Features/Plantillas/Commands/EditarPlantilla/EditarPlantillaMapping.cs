using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;

public static class EditarPlantillaMapping
{
    public static PlantillaEditModel MapearEditPlantilla(EditarPlantillaCommand request)
    {

        var edit = new PlantillaEditModel
        {
            Id = request.Id,
            NombrePlantilla = request.NombrePlantilla,
            Descripcion = request.Descripcion,
            Visibilidad = request.Visibilidad,
            EtapasRequest = request.Etapas!.Select(e => new EtapaEditModel
            {
                Id = e.Id.HasValue ? e.Id.Value : 0,
                Orden = e.OrdenEtapa,
                Nombre = e.NombreEtapa,
                IdTipoEtapa = e.IdTipoEtapa,
                IdTipoAprobador = e.IdTipoAprobador,
                Firmantes = e.Firmantes!.Select(f => new FirmanteEditModel
                {
                    Id = f.Id.HasValue ? f.Id.Value : 0,
                    IdUsuarioAsignado = f.IdUsuarioAsignado,
                    CargoUsuario = f.CargoUsuario,
                    AreaUsuario = f.AreaUsuario,
                    GerenciaUsuario = f.GerenciaUsuario,
                    IdUsuarioOrigen = f.IdUsuarioOrigen
                }).ToList()
            }).ToList()
        };

        return edit;
    }
}

