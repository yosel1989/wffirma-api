using System.Linq.Expressions;
using FluentValidation;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Validators
{
    public static class AccesoEspecialValidationExtensions
    {
        public static void AgregarReglasBasicas<T>(
            this AbstractValidator<T> validator,
            Expression<Func<T, long>> IdUsuario,
            Expression<Func<T, long>> IdArea)
        {
            validator.RuleFor(IdUsuario)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("Usuario es obligatorio y debe ser distinto de cero.")
                .WithErrorCode("USUARIO_INVALID");

            validator.RuleFor(IdArea)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("Area es obligatorio y debe ser distinto de cero.")
                .WithErrorCode("AREA_INVALID");
        }



        public static void PaginadoReglasBasicas<T>(
            this AbstractValidator<T> validator,
            Expression<Func<T, long?>> IdUsuario,
            Expression<Func<T, long?>> IdArea)
        {
            validator.RuleFor(IdUsuario)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("El usuario debe ser distinto de cero.")
                .WithErrorCode("USUARIO_INVALID_ZERO")
                .When(x => IdUsuario.Compile()(x) != null);


            validator.RuleFor(IdArea)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("El área debe ser distinto de cero.")
                .WithErrorCode("AREA_INVALID_ZERO")
                .When(x => IdArea.Compile()(x) != null);
        }


        public static void EliminarReglasBasicas<T>(
            this AbstractValidator<T> validator,
            Expression<Func<T, long>> IdAccesoEspecial)
        {
            validator.RuleFor(IdAccesoEspecial)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("El id del acceso especial debe ser distinto de cero.")
                .WithErrorCode("ACCESP_INVALID_ZERO");
        }

        public static void CambiarEstadoReglasBasicas<T>(
            this AbstractValidator<T> validator,
            Expression<Func<T, long>> IdAccesoEspecial)
        {
            validator.RuleFor(IdAccesoEspecial)
                .Cascade(CascadeMode.Stop)
                .NotEqual(0)
                .WithMessage("El id del acceso especial debe ser distinto de cero.")
                .WithErrorCode("ACCESP_INVALID_ZERO");
        }
    }
}
