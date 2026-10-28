using COFIDE.WFF.Api.Request.AccesosEspeciales;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;

namespace COFIDE.WFF.Api.Mappings
{
    public static class AccesoEspecialRequestMappings
    {
        public static RegistrarAccesoEspecialCommand ToRegistrarCommand(this RegistrarAccesoEspecialRequest r) =>
            new(
                r.IdUsuario,
                r.IdArea
            );


        public static ObtenerAccesoEspecialPaginadoQuery ToQuery(this AccesoEspecialPaginadoRequest r) =>
            new(r.IdArea, r.IdUsuario)
            {
                PageNumber = r.PageNumber,
                PageSize = r.PageSize,
                Sorts = r.Sorts
            };
    }
}
