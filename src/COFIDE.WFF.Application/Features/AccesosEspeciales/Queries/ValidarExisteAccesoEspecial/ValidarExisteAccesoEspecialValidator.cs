using COFIDE.WFF.Application.Features.AccesosEspeciales.Validators;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ValidarExisteAccesoEspecial
{
    public class ValidarExisteAccesoEspecialValidator : AbstractValidator<ValidarExisteAccesoEspecialQuery>
    {
        public ValidarExisteAccesoEspecialValidator()
        {
            this.ValidarExisteReglasBasicas(
                 x => x.IdUsuario,
                 x => x.IdArea);
        }
    }
}
