using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace COFIDE.WFF.Infrastructure.Adapters.Repositories
{
	public partial class SolicitudQueryRepository
	{
		/// <summary>
		/// Define qué solicitudes puede ver el usuario:
		///   lo que ya ve por su rol  OR  las de las áreas con acceso especial activo.
		/// Un acceso a una gerencia incluye sus departamentos; un acceso a un departamento
		/// incluye solo ese departamento.
		/// Debe aplicarse ANTES de los filtros de la bandeja (gerencia, departamento, etc.),
		/// para que esos filtros solo reduzcan lo visible y nunca lo amplíen.
		/// </summary>
		private static IQueryable<Solicitud> AplicarVisibilidad(
			IQueryable<Solicitud> query,
			long? codigoUsuario,
			long? areaUsuario,
			string? roLUsuario,
			List<long>? idsAreasAccesoEspecial)
		{
			Expression<Func<Solicitud, bool>>? porRol = null;

			var rol = RolUsuarioCodigo.Desde(roLUsuario);
			if (rol is not null)
			{
				if (rol == RolUsuario.Coordinador && areaUsuario.HasValue)
				{
					porRol = s =>
						s.UsuarioSolicitante.IdArea.HasValue &&
						s.UsuarioSolicitante.IdArea.Value == areaUsuario.Value;
				}
				else if (rol == RolUsuario.SolicitanteAprobador && codigoUsuario.HasValue)
				{
					porRol = s => s.UsuarioSolicitante.Id == codigoUsuario.Value;
				}
			}

			// Rol sin restricción: ya ve todo, el acceso especial no agrega nada.
			if (porRol is null)
				return query;

			// CA06: sin accesos especiales activos, el comportamiento es el de hoy.
			if (idsAreasAccesoEspecial is not { Count: > 0 })
				return query.Where(porRol);

			var ids = idsAreasAccesoEspecial;

			// CA03 / CA04: el id puede ser una gerencia (padre del área del solicitante)
			// o un departamento (el área del solicitante).
			Expression<Func<Solicitud, bool>> porAcceso = s =>
				s.UsuarioSolicitante.IdArea.HasValue &&
				(ids.Contains(s.UsuarioSolicitante.IdArea.Value) ||
				 (s.UsuarioSolicitante.Area.IdAreaPadre.HasValue &&
				  ids.Contains(s.UsuarioSolicitante.Area.IdAreaPadre.Value)));

			return query.Where(Or(porRol, porAcceso));
		}

		private static Expression<Func<T, bool>> Or<T>(
			Expression<Func<T, bool>> left,
			Expression<Func<T, bool>> right)
		{
			var parametro = left.Parameters[0];
			var cuerpoDerecho = new ReemplazarParametro(right.Parameters[0], parametro).Visit(right.Body)!;

			return Expression.Lambda<Func<T, bool>>(
				Expression.OrElse(left.Body, cuerpoDerecho),
				parametro);
		}

		private sealed class ReemplazarParametro : ExpressionVisitor
		{
			private readonly ParameterExpression _origen;
			private readonly Expression _destino;

			public ReemplazarParametro(ParameterExpression origen, Expression destino)
			{
				_origen = origen;
				_destino = destino;
			}

			protected override Expression VisitParameter(ParameterExpression node)
				=> node == _origen ? _destino : base.VisitParameter(node);
		}
	}
}
