using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
namespace COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerListadoParametro;

public sealed class ObtenerListadoParametroQueryHandler(
        IMapper mapper,
        IParametroRepository parametroRepository
    ) : BaseQueryHandler<ObtenerListadoParametroQuery, List<ListadoParametroDto>>(mapper)
{
    private readonly IParametroRepository _parametroRepository = parametroRepository;

    public override async Task<List<ListadoParametroDto>> Handle(ObtenerListadoParametroQuery request, CancellationToken cancellationToken)
    {
        var entidades = await _parametroRepository
            .ObtenerParametrosAsync(cancellationToken, tracking: false);

        var dto = _mapper.Map<List<ListadoParametroDto>>(entidades);

        return dto;
    }
}
