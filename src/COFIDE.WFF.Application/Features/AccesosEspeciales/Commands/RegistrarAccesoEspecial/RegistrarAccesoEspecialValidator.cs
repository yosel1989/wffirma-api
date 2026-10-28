using COFIDE.WFF.Application.Features.AccesosEspeciales.Validators;
using FluentValidation;
namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Commands.RegistrarAccesoEspecial
{
    public class RegistrarAccesoEspecialValidator : AbstractValidator<RegistrarAccesoEspecialCommand>
    {
        public RegistrarAccesoEspecialValidator()
        {
            this.AgregarReglasBasicas(
                 x => x.IdUsuario,
                 x => x.IdArea);
        }
    }
}
