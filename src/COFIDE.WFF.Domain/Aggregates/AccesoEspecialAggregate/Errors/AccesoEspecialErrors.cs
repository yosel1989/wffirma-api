using COFIDE.WFF.Domain.Common.Errors;

namespace COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Errors
{
    public static class AccesoEspecialErrors
    {
        public static readonly DomainError UsuarioRequired =
            new("ACCESP_USUARIO_REQUIRED", "El usuario es obligatorio.");

        public static readonly DomainError AreaRequired =
            new("ACCESP_AREA_REQUIRED", "El área es obligatorio.");

        public static readonly DomainError AreaInvalid =
            new("ACCESP_AREA_INVALID", "El área debe ser diferente a 0.");
    }
}
