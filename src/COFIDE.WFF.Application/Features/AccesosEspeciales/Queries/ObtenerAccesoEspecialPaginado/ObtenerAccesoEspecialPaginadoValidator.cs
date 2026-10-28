using COFIDE.WFF.Application.Features.AccesosEspeciales.Validators;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado
{
    public class ObtenerAccesoEspecialPaginadoValidator : AbstractValidator<ObtenerAccesoEspecialPaginadoQuery>
    {
        public ObtenerAccesoEspecialPaginadoValidator()
        {
            this.PaginadoReglasBasicas(
                 x => x.IdUsuario,
                 x => x.IdArea);
        }
    }
}
