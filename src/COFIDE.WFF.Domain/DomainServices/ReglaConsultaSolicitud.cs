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
    // La pertenencia se evalua con el area guardada en la solicitud, no con el area actual del usuario.
    public static Expression<Func<Solicitud, bool>> CrearPredicado(
        long? idUsuario,
        long? idArea,
        string? codigoRol,
        IReadOnlyCollection<long>? areasAccesoEspecial)
    {
        var rol = RolUsuarioCodigo.Desde(codigoRol);
        var areas = areasAccesoEspecial?.ToArray() ?? [];

        if (rol == RolUsuario.Administrador)
            return s => true;

        if (rol == RolUsuario.Coordinador)
        {
            if (!idArea.HasValue)
                return s => false;

            var area = idArea.Value;
            return s => s.IdAreaSolicitud == area ||
                areas.Contains(s.IdAreaSolicitud) ||
                areas.Contains(s.IdAreaPadreSolicitud);
        }

        if (rol == RolUsuario.SolicitanteAprobador)
        {
            if (!idUsuario.HasValue)
                return s => false;

            var usuario = idUsuario.Value;
            return s => s.IdUsuarioSolicitante == usuario ||
                areas.Contains(s.IdAreaSolicitud) ||
                areas.Contains(s.IdAreaPadreSolicitud);
        }

        return s => false;
    }
}
