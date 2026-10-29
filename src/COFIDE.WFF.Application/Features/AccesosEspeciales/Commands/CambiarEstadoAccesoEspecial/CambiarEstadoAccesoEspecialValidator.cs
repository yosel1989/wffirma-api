using COFIDE.WFF.Application.Features.AccesosEspeciales.Validators;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.CambiarEstadoAccesoEspecial
{
    public class CambiarEstadoAccesoEspecialValidator : AbstractValidator<CambiarEstadoAccesoEspecialCommand>
    {
        public CambiarEstadoAccesoEspecialValidator()
        {
            this.CambiarEstadoReglasBasicas(
                 x => x.Id);
        }
    }
}
