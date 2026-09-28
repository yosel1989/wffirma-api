using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Usuarios.Queries.UsuarioBuscador;
public sealed record BuscarUsuariosQuery(string Termino) : BaseQuery<List<UsuarioBuscadorDto>>;