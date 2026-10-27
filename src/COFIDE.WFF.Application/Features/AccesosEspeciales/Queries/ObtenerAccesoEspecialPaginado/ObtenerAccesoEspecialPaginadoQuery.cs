using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado
{
    public sealed record ObtenerAccesoEspecialPaginadoQuery
    (
        long? IdGerencia,
        long? IdDepartamento,
        long? IdUsuario
    ) : GetPagedQuery<AccesoEspecialPaginadoResponseDto>;
}
