using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;

namespace COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;

internal sealed class FlujoRestauracion
{
    public required Solicitud Solicitud { get; init; }
    public required Plantilla Plantilla { get; init; }
    public required SolicitudFirmanteFlujo Primero { get; init; }
    public required SolicitudFirmanteFlujo Segundo { get; init; }
    public required SolicitudFirmanteFlujo Tercero { get; init; }
    public required SolicitudDocumento Documento { get; init; }

    public DateTime T1 { get; } = new(2026, 10, 29, 12, 07, 0);
    public DateTime T2 { get; } = new(2026, 10, 29, 12, 33, 0);
    public DateTime T3 { get; } = new(2026, 10, 29, 12, 42, 0);

    public long UsuarioPrimero => Primero.IdUsuarioAsignado;
    public long UsuarioSegundo => Segundo.IdUsuarioAsignado;
    public long UsuarioTercero => Tercero.IdUsuarioAsignado;

    public IReadOnlyList<SolicitudFirmanteFlujo> Firmantes => [Primero, Segundo, Tercero];
}

internal static class RestauracionFlujoMother
{
    public static FlujoRestauracion EnCursoSinRevisiones()
        => Montar();

    public static FlujoRestauracion EnCursoConDosAprobados()
    {
        var flujo = Montar();
        FirmarYAprobar(flujo, flujo.Primero, historialId: 10, flujo.T1);
        FirmarYAprobar(flujo, flujo.Segundo, historialId: 11, flujo.T2);
        flujo.Solicitud.ClearDomainEvents();
        return flujo;
    }

    public static FlujoRestauracion Aprobada()
    {
        var flujo = Montar();
        FirmarYAprobar(flujo, flujo.Primero, historialId: 10, flujo.T1);
        FirmarYAprobar(flujo, flujo.Segundo, historialId: 11, flujo.T2);
        FirmarYAprobar(flujo, flujo.Tercero, historialId: 12, flujo.T3);
        flujo.Solicitud.ClearDomainEvents();
        return flujo;
    }

    public static FlujoRestauracion RechazadaEnTercero()
    {
        var flujo = Montar();
        FirmarYAprobar(flujo, flujo.Primero, historialId: 10, flujo.T1);
        FirmarYAprobar(flujo, flujo.Segundo, historialId: 11, flujo.T2);

        using (HoraSistema.Fijar(flujo.T3))
        {
            flujo.Solicitud.RechazarSolicitud(
                flujo.UsuarioTercero,
                "Rechazador",
                "Documentación incompleta",
                flujo.Tercero.IdPlantillaEtapa,
                "GTI",
                idHistorialRevision: 12,
                ciclo: 1);
        }

        flujo.Solicitud.ClearDomainEvents();
        return flujo;
    }

    public static Func<long> IdsDetalle(long inicio = 900) => SolicitudMother.Ids(inicio);

    private static FlujoRestauracion Montar()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1, 2, 3);
        var solicitud = SolicitudMother.EnCurso();
        var solicitudPlantilla = SolicitudPlantilla.Crear(1, solicitud, plantilla);
        solicitudPlantilla.ActualizarPlantilla(plantilla.Id);
        solicitud.AsignarPlantilla(solicitudPlantilla);

        new WorkflowDomainService().AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        var documento = solicitud.AgregarDocumento(1);
        solicitud.GenerarDetallesFirmaPendientes(SolicitudMother.Ids(800));
        solicitud.ClearDomainEvents();

        var firmantes = solicitud.SolicitudPlantilla.FirmantesFlujo.OrderBy(f => f.OrdenEtapa).ToList();

        return new FlujoRestauracion
        {
            Solicitud = solicitud,
            Plantilla = plantilla,
            Primero = firmantes[0],
            Segundo = firmantes[1],
            Tercero = firmantes[2],
            Documento = documento
        };
    }

    private static void FirmarYAprobar(
        FlujoRestauracion flujo,
        SolicitudFirmanteFlujo firmante,
        long historialId,
        DateTime fecha)
    {
        using (HoraSistema.Fijar(fecha))
        {
            var detalle = flujo.Documento.DetallesFirma.Single(d =>
                d.Estado && d.IdSolicitudFirmanteFlujo == firmante.Id);

            detalle.Firmar(
                $"doc-{firmante.IdUsuarioAsignado}",
                "pdf",
                "/firmados",
                $"hash-{firmante.IdUsuarioAsignado}",
                true);

            flujo.Solicitud.AprobarFirmante(
                firmante.IdUsuarioAsignado,
                "Aprobador",
                historialId,
                ciclo: 1);
        }
    }
}
