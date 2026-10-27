using System.Linq.Expressions;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Validators
{
    public static class AccesoEspecialValidationExtensions
    {
        public static void AgregarReglasBasicas<T>(
            this AbstractValidator<T> validator,
            Expression<Func<T, long>> IdUsuario,
            Expression<Func<T, long>> IdGerencia,
            Expression<Func<T, long?>> IdDepartamento)
        {
            validator.RuleFor(IdUsuario)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("Usuario es obligatorio y debe ser distinto de cero.")
                .WithErrorCode("USUARIO_INVALID");

            validator.RuleFor(IdGerencia)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("Gerencia es obligatorio y debe ser distinto de cero.")
                .WithErrorCode("GERENCIA_INVALID");

            validator.RuleFor(IdDepartamento)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Departamento es obligatorio y debe ser distinto de cero.")
                .WithErrorCode("DEPARTAMENTO_REQUIRED");
        }
    }
}
