using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Ports.Out;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Dtos;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Errors;
using COFIDE.WFF.Domain.Entities;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario;

public sealed class ObtenerContextoUsuarioQueryHandler(
        IMapper mapper,
        ICurrentUserService currentUser,
        IAutenticacionExternaPort authPort,
        IUsuarioRepository usuarioRepository
    ) : BaseQueryHandler<ObtenerContextoUsuarioQuery, UsuarioContextoDto?>(mapper)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IAutenticacionExternaPort _authPort = authPort;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

    public override async Task<UsuarioContextoDto?> Handle(ObtenerContextoUsuarioQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserIdLocal is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        if (_currentUser.CodigoUsuario is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.UserNotAuthenticated);

        var codigoUsuario = _currentUser.CodigoUsuario;

        var usuarioSuc = await _authPort.AutenticarAsync(codigoUsuario, cancellationToken);

        if (usuarioSuc is null)
            throw new ValidacionAplicacion.ApplicationException(ObtenerContextoUsuarioErrors.UsuarioValidadoEnSuc.Format(codigoUsuario));

        var usuario = await _usuarioRepository.GetByCuentaLogueoAsync(codigoUsuario);

        if (usuario is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.AccessDenied);

        var (idDepartamento, idGerencia) = ResolverArea(usuario.Area);

        return new UsuarioContextoDto
        {
            CodigoSistema = usuarioSuc.CodigoSistema,
            NombreSistema = usuarioSuc.NombreSistema,
            CodigoUsuario = usuarioSuc.CodigoUsuario,
            NombreUsuario = usuarioSuc.NombreUsuario,
            CodigoRolUsuario = usuarioSuc.CodigoRolUsuario,
            NombreRolUsuario = usuarioSuc.NombreRolUsuario,
            Correo = usuarioSuc.Correo,
            AreaId = usuario.IdArea,
            NombreArea = usuario.Area?.NombreArea,
            CargoId = usuario.IdCargo,
            NombreCargo = usuario.Cargo?.NombreCargo,
            GerenciaId = idGerencia,
            DepartamentoId = idDepartamento,

            LstMenu = usuarioSuc.LstMenu
        .Select(x => new MenuDto
        {
            Id = x.Id,
            IdMenuPadre = x.IdMenuPadre,
            Nombre = x.Nombre,
            Descripcion = x.Descripcion ?? string.Empty,
            DescripcionCompleta = x.DescripcionCompleta ?? string.Empty,
            Url = x.Url ?? string.Empty,
            Icono = x.Icono ?? string.Empty,
        })
        .ToList(),
            LstSubMenu = usuarioSuc.LstSubMenu
        .Select(x => new MenuDto
        {
            Id = x.Id,
            IdMenuPadre = x.IdMenuPadre,
            Nombre = x.Nombre,
            Descripcion = x.Descripcion ?? string.Empty,
            DescripcionCompleta = x.DescripcionCompleta ?? string.Empty,
            Url = x.Url ?? string.Empty,
            Icono = x.Icono ?? string.Empty
        })
        .ToList(),
        };
    }

    private static (long? idDepartamento, long? idGerencia) ResolverArea(Area? area)
    {
        if (area == null)
            return (null, null);

        return area.Tipo switch
        {
            'G' => (null, area.Id),

            'D' => (area.Id, area.AreaPadre?.Id),

            _ => (null, null)
        };
    }
}
