using COFIDE.WFF.Domain.Common.Entities;

namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class SolicitudSustento : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public string NombreSustento { get; private set; }
    public string ExtensionSustento { get; private set; }
    public string RutaSustento { get; private set; }
    public long? PesoSustento { get; private set; }

    public bool EsNuevo { get; private set; } = false;

    private SolicitudSustento()
    {
        NombreSustento = null!;
        ExtensionSustento = null!;
        RutaSustento = null!;
    }

    internal static SolicitudSustento Crear(
        long idSolicitud,
        long idSolicitudSustento,
        string nombreSustento,
        string extensionSustento,
        string rutaSustento,
        long? pesoSustento)
    {
        var sustento = new SolicitudSustento
        {
            Id = idSolicitudSustento,
            IdSolicitud = idSolicitud,
            NombreSustento = nombreSustento,
            ExtensionSustento = extensionSustento,
            RutaSustento = rutaSustento,
            PesoSustento = pesoSustento,
            Estado = true
        };

        sustento.MarcarComoNuevo();

        return sustento;
    }

    public void Inactivar()
    {
        Estado = false;
    }
    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }
}
