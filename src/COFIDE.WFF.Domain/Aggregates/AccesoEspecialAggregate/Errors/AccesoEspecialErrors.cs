using COFIDE.WFF.Domain.Common.Errors;

namespace COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Errors
{
    public static class AccesoEspecialErrors
    {
        public static readonly DomainError UsuarioRequired =
            new("ACCESP_USUARIO_REQUIRED", "El usuario es obligatorio.");

        public static readonly DomainError GerenciaRequired =
            new("ACCESP_GERENCIA_REQUIRED", "El mensaje es obligatorio.");

        public static readonly DomainError DepartamentoInvalid =
            new("ACCESP_DEPARTAMENTO_INVALID", "El departamento debe ser diferente a 0.");
    }
}
