using FluentValidation;
namespace COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;

public class ValidarExistenciaValidator : AbstractValidator<ValidarExistenciaQuery>
{
    public ValidarExistenciaValidator()
    {
        RuleFor(x => x.NombrePlantilla)
          .Cascade(CascadeMode.Stop)
          .NotNull().WithMessage("Nombre de Plantilla es obligatorio.").WithErrorCode("PLANTILLA_NAME_REQUERID");

    }
}