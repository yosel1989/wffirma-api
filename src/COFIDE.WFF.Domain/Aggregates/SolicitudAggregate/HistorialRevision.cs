using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Common.Entities;

namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class HistorialRevision : AuditableEntity<long>
{
    #region Propiedades unicas
    public long IdSolFirmanteFlujo { get; private set; }
    public long IdFirmanteFlujo { get; private set; }
    public long IdSolicitudPlantilla { get; private set; }
    public long IdPlantillaEtapa { get; private set; }
    public bool RevAprobado { get; private set; }
    public DateTime? FechaRevision { get; private set; }
    public int? Ciclo { get; private set; }

    #endregion

    #region Relaciones
    public SolicitudFirmanteFlujo SolFirmanteFlujo { get; private set; } = default!;
    public FirmanteFlujo FirmanteFlujo { get; private set; } = default!;
    public SolicitudPlantilla SolicitudPlantilla { get; private set; } = default!;

    #endregion 


    #region Propiedades de Historial Revision, no considerarse en la persistencia
    public bool EsNuevo { get; private set; } = false;

    #endregion 


    private HistorialRevision() { }
    private HistorialRevision(
      long IdHistorialRevision,
      long idSolicitudFirmanteFlujo,
      long idFirmanteFlujo,
      long idSolicitudPlantilla,
      long idPlantillaEtapa,
      bool revAprobado,
      DateTime? fechaRevision,
      int ciclo)
    {
        Id = IdHistorialRevision;
        IdSolFirmanteFlujo = idSolicitudFirmanteFlujo;
        IdFirmanteFlujo = idFirmanteFlujo;
        IdSolicitudPlantilla = idSolicitudPlantilla;
        IdPlantillaEtapa = idPlantillaEtapa;
        RevAprobado = revAprobado;
        Estado = true;
        FechaRevision = fechaRevision;
        Ciclo = ciclo;
    }

    public static HistorialRevision Crear(
      long IdHistorialRevision,
      long idSolicitudFirmanteFlujo,
      long idFirmanteFlujo,
      long idSolicitudPlantilla,
      long idPlantillaEtapa,
      bool revAprobado,
      DateTime? fechaRevision,
      int ciclo)
    {
        return new HistorialRevision
        (
            IdHistorialRevision, idSolicitudFirmanteFlujo, idFirmanteFlujo, idSolicitudPlantilla, idPlantillaEtapa, revAprobado, fechaRevision, ciclo
        );
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }


}
