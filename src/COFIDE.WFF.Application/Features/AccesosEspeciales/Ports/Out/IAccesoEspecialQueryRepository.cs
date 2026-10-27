using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;

namespace COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out
{
    public interface IAccesoEspecialQueryRepository
    {

        #region PAGINADO

        Task<PagedResult<AccesoEspecialPaginadoDto, AccesoEspecialResumenPaginadoDto>> ObtenerPaginado(
            PagedQueryParams paginationParams,
            long? idGerencia,
            long? idDepartamento,
            long? idUsuario,
            CancellationToken cancellationToken);

        #endregion
    }
}
