using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.ValueObjects;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
public class SolicitudCorreo : AuditableEntity<long>
{
    public long IdSolicitud { get; private set; }
    public CorreoElectronico Correo { get; private set; } = default!;
    public bool EsNuevo { get; private set; } = false;
    private SolicitudCorreo() { }

    private SolicitudCorreo(
      long idSolicitud,
      long idSolicitudCorreo,
      CorreoElectronico correo)
    {
        Id = idSolicitudCorreo;
        IdSolicitud = idSolicitud;
        Correo = correo;
        Estado = true;
    }

    public static SolicitudCorreo Crear(
       long idSolicitud,
       long idSolicitudCorreo,
       string correo)
    {
        var correoVO = CorreoElectronico.Crear(correo);

       var solicitudCorreo = new SolicitudCorreo(
            idSolicitud,
            idSolicitudCorreo,
            correoVO);

        solicitudCorreo.MarcarComoNuevo();

        return solicitudCorreo;
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
