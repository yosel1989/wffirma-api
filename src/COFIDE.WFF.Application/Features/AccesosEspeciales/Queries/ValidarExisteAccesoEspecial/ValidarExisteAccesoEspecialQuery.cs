using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ValidarExisteAccesoEspecial
{
    public sealed record ValidarExisteAccesoEspecialQuery
    (
        long IdArea,
        long IdUsuario
    ) : BaseQuery<bool>;
}
