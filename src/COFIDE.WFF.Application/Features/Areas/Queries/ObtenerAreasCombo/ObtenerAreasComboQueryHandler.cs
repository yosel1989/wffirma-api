using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Areas.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;

public sealed class ObtenerAreasComboQueryHandler(
        IMapper mapper,
        IAreaRepository areaRepository,
        ICurrentUserService currentUser
    ) : BaseQueryHandler<ObtenerAreasComboQuery, List<AreasComboDto>>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IAreaRepository _areaRepository = areaRepository;

    public override async Task<List<AreasComboDto>> Handle(
        ObtenerAreasComboQuery request, CancellationToken cancellationToken)
    {
        List<long> excluirIds = new List<long>();
        // mejora el if

        if(request.excluirAreaUsuario == true)
        {
            if (_currentUser.UserIdLocal is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            if (_currentUser.UserCargoId is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

            var areaUsuario = _currentUser.UserAreaId ?? 0;
            excluirIds.Add(areaUsuario);
        }

        var dtoItems = await _areaRepository.GetAreaCombo(excluirIds, request.Tipo, cancellationToken);
        return dtoItems;
    }
}