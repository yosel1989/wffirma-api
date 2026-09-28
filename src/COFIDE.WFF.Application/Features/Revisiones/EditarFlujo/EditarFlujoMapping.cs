using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Inputs;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
namespace COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;

public static class EditarFlujoMapping
{
    public static CrearPlantillaInput MapearInputPlantilla(EditarFlujoCommand request, Plantilla plantilla)
    {
        var input = new CrearPlantillaInput
        {
            NombrePlantilla = plantilla.NombrePlantilla!.Valor,
            Descripcion = plantilla.Descripcion!.Valor,
            Visibilidad = plantilla.Visibilidad ?? false,
            EsReutilizable = plantilla.EsReutilizable ?? false,

            Etapas = request.Etapas!
                        .Select(e => new EtapaInput
                        {

                            OrdenEtapa = e.OrdenEtapa,
                            NombreEtapa = e.NombreEtapa,
                            IdTipoEtapa = e.IdTipoEtapa,
                            IdTipoAprobador = e.IdTipoAprobador,

                            Firmantes = e.Firmantes!
                                .Select(f => new FirmanteInput
                                {
                                    IdUsuarioAsignado = f.IdUsuarioAsignado,
                                    CargoUsuario = f.CargoUsuario,
                                    AreaUsuario = f.AreaUsuario,
                                    GerenciaUsuario = f.GerenciaUsuario,
                                    IdUsuarioOrigen = f.IdUsuarioOrigen
                                })
                                .ToList()
                        })
                        .ToList()
        };

        return input;
    }
    public static PlantillaEditModel MapearEditPlantilla(EditarFlujoCommand request)
    {
        var etapasCorregidas = request.Etapas!
                   .Select(e => e.TempId == Guid.Empty
                       ? e with { TempId = Guid.NewGuid() }
                       : e)
                   .ToList();

        request = request with { Etapas = etapasCorregidas };

        var edit = new PlantillaEditModel
        {
            Id = request.Id,
            EtapasRequest = request.Etapas!.Select(e => new EtapaEditModel
            {
                Id = e.Id,
                TempId = e.TempId,
                Orden = e.OrdenEtapa,
                Nombre = e.NombreEtapa,
                IdTipoEtapa = e.IdTipoEtapa,
                IdTipoAprobador = e.IdTipoAprobador,
                Firmantes = e.Firmantes!.Select(f => new FirmanteEditModel
                {
                    Id = f.Id,
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
