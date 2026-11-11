using COFIDE.WFF.Api.Request.Plantillas;
using COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Queries.GestionPlantillas;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ObtenerPlantillaPaginado;

namespace COFIDE.WFF.Api.Mappings
{
    /// <summary>Request (contrato HTTP) → Command/Query (contrato de Application) para Plantillas.</summary>
    public static class PlantillaRequestMappings
    {
        public static RegistrarPlantillaCommand ToCommand(this CrearPlantillaRequest r) =>
            new(r.NombrePlantilla, r.Descripcion, r.Visibilidad, r.EsReutilizable, r.Etapas);

        public static EditarPlantillaCommand ToCommand(this ActualizarPlantillaRequest r, long id) =>
            new(id, r.NombrePlantilla, r.Descripcion, r.Etapas, r.Visibilidad);

        public static ObtenerPlantillaPaginadoQuery ToQuery(this BuscarPlantillasRequest r) =>
            new(r.Busqueda, r.Visibilidad, r.Aprobadores, r.Gerencias)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };

        public static GestionPlantillasQuery ToQuery(this GestionPlantillasRequest r) =>
            new(r.Fechainicio, r.Fechafin, r.Gerencia, r.Departamento, r.Creador, r.Aprobador, r.NombrePlantilla)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };
    }
}
