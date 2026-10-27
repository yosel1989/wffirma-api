using COFIDE.WFF.Api.Request.AccesosEspeciales;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;

namespace COFIDE.WFF.Api.Mappings
{
    public static class AccesoEspecialRequestMappings
    {
        public static RegistrarAccesoEspecialCommand ToRegistrarCommand(this RegistrarAccesoEspecialRequest r) =>
            new(
                r.IdUsuario,
                r.IdGerencia,
                r.IdDepartamento);


        public static ObtenerAccesoEspecialPaginadoQuery ToQuery(this AccesoEspecialPaginadoRequest r) =>
            new(r.IdGerencia, r.IdDepartamento, r.IdUsuario)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };
    }
}
