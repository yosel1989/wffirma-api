using COFIDE.WFF.Application.Common.Queries;
using COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario.Dtos;

namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerContextoUsuario;
public record ObtenerContextoUsuarioQuery : BaseQuery<UsuarioContextoDto?>;