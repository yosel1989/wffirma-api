using System.Linq.Expressions;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Enums;
using COFIDE.WFF.Domain.Exceptions;

namespace COFIDE.WFF.Domain.DomainServices;

public static class ReglaConsultaSolicitud
{
    public static void Validar(
        Solicitud? solicitud,
        long? idUsuario,
        long? idArea,
        string? codigoRol,
        IReadOnlyCollection<long>? areasAccesoEspecial)
    {
        if (solicitud is null || !CrearPredicado(idUsuario, idArea, codigoRol, areasAccesoEspecial)
                .Compile()(solicitud))
            throw new DomainException(SolicitudErrors.PermisoVisualizarSolicitud);
    }

    // La expresion permite aplicar la misma regla en memoria y antes de paginar en BD.
    public static Expression<Func<Solicitud, bool>> CrearPredicado(
        long? idUsuario,
        long? idArea,
        string? codigoRol,
        IReadOnlyCollection<long>? areasAccesoEspecial)
    {
        var rol = RolUsuarioCodigo.Desde(codigoRol);
        var areas = areasAccesoEspecial?.Select(id => (long?)id).ToArray() ?? [];

        if (rol == RolUsuario.Coordinador && idArea.HasValue)
        {
            var area = idArea.Value;
            if (areas.Length == 0)
                return s => s.UsuarioSolicitante.IdArea == area;

            return s => s.UsuarioSolicitante.IdArea == area ||
                areas.Contains(s.UsuarioSolicitante.IdArea) ||
                (s.UsuarioSolicitante.Area != null && areas.Contains(s.UsuarioSolicitante.Area.IdAreaPadre));
        }

        if (rol == RolUsuario.SolicitanteAprobador && idUsuario.HasValue)
        {
            var usuario = idUsuario.Value;
            if (areas.Length == 0)
                return s => s.IdUsuarioSolicitante == usuario;

            return s => s.IdUsuarioSolicitante == usuario ||
                areas.Contains(s.UsuarioSolicitante.IdArea) ||
                (s.UsuarioSolicitante.Area != null && areas.Contains(s.UsuarioSolicitante.Area.IdAreaPadre));
        }

        // Conserva la visibilidad previa para administradores y contextos sin restriccion por rol.
        return s => true;
    }
}
