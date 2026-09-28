using FluentValidation;
using System.Linq.Expressions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Validators;

public static class SolicitudValidationExtensions
{
    public static void AgregarReglasBasicas<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, long>> tipoSolicitud,
        Expression<Func<T, long>> subTipoSolicitud,
        Expression<Func<T, long>> tipoOperacion,
        Expression<Func<T, string?>> asunto)
    {
        validator.RuleFor(tipoSolicitud)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0)
            .WithMessage("Tipo de Solicitud es obligatorio y debe ser distinto de cero.")
            .WithErrorCode("TIPO_SOLICITUD_INVALID");

        validator.RuleFor(subTipoSolicitud)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0)
            .WithMessage("SubTipo de Solicitud es obligatorio y debe ser distinto de cero.")
            .WithErrorCode("SUB_TIPO_SOLICITUD_INVALID");

        validator.RuleFor(tipoOperacion)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0)
            .WithMessage("Tipo de Operación es obligatorio y debe ser distinto de cero.")
            .WithErrorCode("TIPO_OPERACION_INVALID");

        validator.RuleFor(asunto)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Asunto es obligatorio.")
            .WithErrorCode("ASUNTO_SOLICITUD_REQUIRED");
    }
}
