using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Areas.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;

namespace COFIDE.WFF.Application.Features.Areas.Queries.ObtenerAreasCombo;

public sealed class ObtenerAreasComboQueryHandler(
        IMapper mapper,
        IAreaRepository areaRepository,
        IUsuarioRepository usuarioRepository
    ) : BaseQueryHandler<ObtenerAreasComboQuery, List<AreasComboDto>>(mapper)
{
    private readonly IAreaRepository _areaRepository = areaRepository;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public override async Task<List<AreasComboDto>> Handle(
        ObtenerAreasComboQuery request, CancellationToken cancellationToken)
    {
        List<long> excluirIds = new List<long>();

        if (request.excluirAreaUsuario is long usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId, tracking: false);
            if(usuario is null)
                throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UsuarioNotFound);

            excluirIds.Add(usuario.IdArea ?? 0);
        }

        var dtoItems = await _areaRepository.GetAreaCombo(excluirIds, request.Tipo, cancellationToken);
        return dtoItems;
    }
}