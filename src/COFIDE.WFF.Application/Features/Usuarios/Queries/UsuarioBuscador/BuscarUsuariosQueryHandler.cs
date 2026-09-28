using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
namespace COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;

public sealed class BuscarUsuariosQueryHandler(
        IMapper mapper,
        IUsuarioRepository usuarioRepository
    ) : BaseQueryHandler<BuscarUsuariosQuery, List<UsuarioBuscadorDto>>(mapper)
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public override async Task<List<UsuarioBuscadorDto>> Handle(BuscarUsuariosQuery request, CancellationToken cancellationToken)
    {
        var dtoItems = await _usuarioRepository.BuscarAsync(request.Termino, cancellationToken);
        return dtoItems;
    }
}
