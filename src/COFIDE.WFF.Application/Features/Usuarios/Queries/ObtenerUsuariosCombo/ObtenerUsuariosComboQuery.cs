using COFIDE.WFF.Application.Common.Queries;

namespace COFIDE.WFF.Application.Features.Usuarios.Queries.ObtenerUsuariosCombo;

public sealed record ObtenerUsuariosComboQuery(bool? Todos = null) : BaseQuery<List<UsuariosComboDto>>;