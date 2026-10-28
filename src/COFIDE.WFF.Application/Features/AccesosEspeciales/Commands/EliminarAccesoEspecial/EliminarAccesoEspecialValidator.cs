using COFIDE.WFF.Application.Features.AccesosEspeciales.Validators;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.EliminarAccesoEspecial
{
    public class EliminarAccesoEspecialValidator : AbstractValidator<EliminarAccesoEspecialCommand>
    {
        public EliminarAccesoEspecialValidator()
        {
            this.EliminarReglasBasicas(
                 x => x.Id);
        }
    }
}
