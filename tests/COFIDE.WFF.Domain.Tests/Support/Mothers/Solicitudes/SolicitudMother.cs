using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

namespace COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;

internal static class SolicitudMother
{
    public static Solicitud Borrador(
        long id = 1,
        string codigo = "GTI-SOL-00000001-2026",
        bool esUrgente = false,
        DateTime? fechaLimite = null,
        decimal? pesoTotalDocu = 0) =>
        Solicitud.CrearBorrador(
            id: id,
            codigoSolicitud: codigo,
            idTipoSolicitud: 1,
            idSubTipoSolicitud: 2,
            idUsuarioSolicitante: 10,
            idTipoOperacion: 3,
            idUsuarioAccion: 10,
            idAreaSolicitud: 5,
            idAreaPadreSolicitud: 2,
            asunto: "Contrato de fideicomiso",
            mensaje: "Mensaje de prueba",
            esUrgente: esUrgente,
            inCodigo: false,
            fechaLimite: fechaLimite,
            pesoTotalDocu: pesoTotalDocu);

    public static Solicitud EnCurso(long id = 1)
    {
        var solicitud = Borrador(id);
        solicitud.Finalizar("Contrato", null, false, false, null, 0, "ana@cofide.com.pe");
        solicitud.ClearDomainEvents();
        return solicitud;
    }

    /// <summary>Agrega un documento con los 14 parámetros resueltos por defecto.</summary>
    public static SolicitudDocumento AgregarDocumento(
        this Solicitud solicitud,
        long id,
        string nombre = "contrato",
        string extension = "pdf",
        string hash = "hash-original",
        long peso = 1000)
    {
        solicitud.AgregarDocumento(
            id, nombre, extension, "/documentos", hash,
            null, null, null, null,
            null, null, null, null,
            peso);

        return solicitud.Documentos.Single(d => d.Id == id);
    }

    /// <summary>Generador incremental para los Func&lt;long&gt; de IDs.</summary>
    public static Func<long> Ids(long inicio = 100)
    {
        var siguiente = inicio;
        return () => siguiente++;
    }
}
