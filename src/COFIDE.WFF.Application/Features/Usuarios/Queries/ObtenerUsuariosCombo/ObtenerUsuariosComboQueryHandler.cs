using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;

public sealed class ObtenerUsuariosComboQueryHandler(
        IMapper mapper,
        IUsuarioRepository usuarioRepository
    ) : BaseQueryHandler<ObtenerUsuariosComboQuery, List<UsuariosComboDto>>(mapper)
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public override async Task<List<UsuariosComboDto>> Handle(ObtenerUsuariosComboQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _usuarioRepository.GetUsuarioCombo(cancellationToken);
        return dtoItems;
    }
}