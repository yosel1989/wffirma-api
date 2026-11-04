using COFIDE.WFF.Domain.ValueObjects;

namespace COFIDE.WFF.Domain.DomainServices;

public enum EstadoRevisionFirmante
{
    Pendiente,
    Aprobado,
    Rechazado
}


public static class ReglaEstadoFirmante
{

    public static EstadoRevisionFirmante Determinar(
        long idEstadoSolicitud,
        int? cicloActual,
        int? ultimoCiclo,
        bool? ultimoAprobado,
        bool revAprobado,
        DateTime? fechaRevision)
    {
        var revisoEnCicloActual = ultimoCiclo != null && ultimoCiclo == cicloActual;
        if (!revisoEnCicloActual)
            return EstadoRevisionFirmante.Pendiente;

        var revisionVigente = revAprobado && fechaRevision != null;
        var solicitudRechazada = idEstadoSolicitud == EstadoSolicitud.Rechazado.Codigo;
        var solicitudAprobadaOEnCurso =
            idEstadoSolicitud == EstadoSolicitud.Aprobado.Codigo
            || idEstadoSolicitud == EstadoSolicitud.EnCurso.Codigo;

        if (ultimoAprobado == true
            && ((revisionVigente && solicitudAprobadaOEnCurso) || (!revisionVigente && solicitudRechazada)))
            return EstadoRevisionFirmante.Aprobado;

        if (ultimoAprobado == false && !revisionVigente && solicitudRechazada)
            return EstadoRevisionFirmante.Rechazado;

        return EstadoRevisionFirmante.Pendiente;
    }
}
