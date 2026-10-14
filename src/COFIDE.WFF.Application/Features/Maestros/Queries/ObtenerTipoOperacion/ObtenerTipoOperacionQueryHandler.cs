using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacion;

public sealed class ObtenerTipoOperacionQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IMaestroRepository tipoOperacionRepository
    ) : BaseQueryHandler<ObtenerTipoOperacionQuery, List<TipoOperacionDto>>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IMaestroRepository _tipoOperacionRepository = tipoOperacionRepository;

    public override async Task<List<TipoOperacionDto>> Handle(ObtenerTipoOperacionQuery request, CancellationToken cancellationToken)
    {
        long idArea = _currentUser.UserAreaId
            ?? throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UsuarioAreaAsignado);

        var data = await _tipoOperacionRepository.GetByIdAreaAsync(idArea, request.Estado, cancellationToken);
        var dtoItems = _mapper.Map<List<TipoOperacionDto>>(data);
        return dtoItems;
    }
}
