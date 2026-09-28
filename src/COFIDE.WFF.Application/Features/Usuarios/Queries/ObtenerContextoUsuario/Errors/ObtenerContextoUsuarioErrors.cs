using COFIDE.WFF.Application.Common.Errors;

namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Errors;

public static class ObtenerContextoUsuarioErrors
{
    public static readonly ApplicationError UsuarioValidadoEnSuc =
        new("USUARIO_NO_VALIDO_SUC",
            "$Usuario {0} no válido en Suc.");

   public static readonly ApplicationError IdPlantillaEtapaInvalido =
       new("PLANTILLA_ETAPA_INVALID_ID",
           "Id Plantilla Etapa inválido.");
}