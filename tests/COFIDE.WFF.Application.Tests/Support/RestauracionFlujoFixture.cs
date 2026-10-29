using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.DomainServices;
using Vo = COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;

namespace COFIDE.WFF.Application.Tests.Support;

internal sealed class FlujoRestauracionApp
{
    public required Solicitud Solicitud { get; init; }
    public required Plantilla Plantilla { get; init; }
    public required SolicitudFirmanteFlujo Primero { get; init; }
    public required SolicitudFirmanteFlujo Segundo { get; init; }
    public required SolicitudFirmanteFlujo Tercero { get; init; }

    public DateTime T1 { get; } = new(2026, 10, 29, 12, 07, 0);
    public DateTime T2 { get; } = new(2026, 10, 29, 12, 33, 0);
    public DateTime T3 { get; } = new(2026, 10, 29, 12, 42, 0);
}

internal static class RestauracionFlujoFixture
{
    public static FlujoRestauracionApp RechazadaEnTercero()
    {
        var flujo = Montar();
        FirmarYAprobar(flujo, flujo.Primero, 10, flujo.T1);
        FirmarYAprobar(flujo, flujo.Segundo, 11, flujo.T2);

        using (HoraSistema.Fijar(flujo.T3))
        {
            flujo.Solicitud.RechazarSolicitud(
                flujo.Tercero.IdUsuarioAsignado,
                "Rechazador",
                "Documentación incompleta",
                flujo.Tercero.IdPlantillaEtapa,
                "GTI",
                12,
                1);
        }

        flujo.Solicitud.ClearDomainEvents();
        return flujo;
    }

    public static FlujoRestauracionApp Aprobada()
    {
        var flujo = Montar();
        FirmarYAprobar(flujo, flujo.Primero, 10, flujo.T1);
        FirmarYAprobar(flujo, flujo.Segundo, 11, flujo.T2);
        FirmarYAprobar(flujo, flujo.Tercero, 12, flujo.T3);
        flujo.Solicitud.ClearDomainEvents();
        return flujo;
    }

    private static FlujoRestauracionApp Montar()
    {
        var plantilla = Plantilla.Crear(7, "Flujo Aprobacion", "Descripcion", false, 10, "GTI", true);
        long etapaId = 1, firmanteId = 1, usuarioId = 100;
        foreach (var orden in new long[] { 1, 2, 3 })
        {
            var etapa = plantilla.AgregarEtapas(
                etapaId, orden, $"Etapa {orden}",
                Vo.TipoEtapa.Secuencial, idTipoEtapa: 1,
                Vo.TipoAprobador.Firmante, idTipoAprobador: 10);
            etapa.AgregarFirmante(firmanteId++, usuarioId++, "Jefe", "GTI", "GTI", 10);
            etapaId++;
        }

        var solicitud = Solicitud.CrearBorrador(
            id: 8,
            codigoSolicitud: "GTI-SOL-00000008-2026",
            idTipoSolicitud: 1,
            idSubTipoSolicitud: 2,
            idUsuarioSolicitante: 10,
            idTipoOperacion: 3,
            idUsuarioAccion: 10,
            idAreaSolicitud: 5,
            idAreaPadreSolicitud: 2,
            asunto: "Contrato de fideicomiso",
            mensaje: "Mensaje de prueba",
            esUrgente: false,
            inCodigo: false,
            fechaLimite: null,
            pesoTotalDocu: 0);

        solicitud.Finalizar("Contrato", null, false, false, null, 0, "ana@cofide.com.pe");

        var solicitudPlantilla = SolicitudPlantilla.Crear(1, solicitud, plantilla);
        solicitudPlantilla.ActualizarPlantilla(plantilla.Id);
        solicitud.AsignarPlantilla(solicitudPlantilla);

        new WorkflowDomainService().AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, Ids(500));

        solicitud.AgregarDocumento(
            1, "contrato", "pdf", "/documentos", "hash-original",
            null, null, null, null,
            null, null, null, null,
            1000);
        solicitud.GenerarDetallesFirmaPendientes(Ids(800));
        solicitud.ClearDomainEvents();

        var firmantes = solicitud.SolicitudPlantilla.FirmantesFlujo.OrderBy(f => f.OrdenEtapa).ToList();

        return new FlujoRestauracionApp
        {
            Solicitud = solicitud,
            Plantilla = plantilla,
            Primero = firmantes[0],
            Segundo = firmantes[1],
            Tercero = firmantes[2]
        };
    }

    private static void FirmarYAprobar(
        FlujoRestauracionApp flujo,
        SolicitudFirmanteFlujo firmante,
        long historialId,
        DateTime fecha)
    {
        using (HoraSistema.Fijar(fecha))
        {
            var documento = flujo.Solicitud.Documentos.Single();
            var detalle = documento.DetallesFirma.Single(d =>
                d.Estado && d.IdSolicitudFirmanteFlujo == firmante.Id);

            detalle.Firmar(
                $"doc-{firmante.IdUsuarioAsignado}",
                "pdf",
                "/firmados",
                $"hash-{firmante.IdUsuarioAsignado}",
                true);

            flujo.Solicitud.AprobarFirmante(firmante.IdUsuarioAsignado, "Aprobador", historialId, 1);
        }
    }

    private static Func<long> Ids(long inicio)
    {
        var siguiente = inicio;
        return () => siguiente++;
    }
}
