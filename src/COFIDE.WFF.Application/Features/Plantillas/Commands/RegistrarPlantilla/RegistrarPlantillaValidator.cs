using FluentValidation;
namespace COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;

public class RegistrarPlantillaValidator : AbstractValidator<RegistrarPlantillaCommand>
{
    public RegistrarPlantillaValidator()
    {
        RuleFor(x => x.NombrePlantilla)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Nombre de Plantilla es obligatorio.")
            .WithErrorCode("PLANTILLA_NAME_REQUIRED")
            .When(x => x.EsReutilizable.HasValue && x.EsReutilizable.Value);

        RuleFor(x => x.Descripcion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Descripción de Plantilla es obligatoria.")
            .WithErrorCode("PLANTILLA_DESCRIPTION_REQUIRED")
            .When(x => x.EsReutilizable.HasValue && x.EsReutilizable.Value);
    }
}