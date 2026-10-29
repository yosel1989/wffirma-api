using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class SolicitudFirmanteFlujo : AuditableEntity<long>
{
    #region Propiedades unicas
    public long IdFirmanteFlujo { get; private set; }
    public long IdSolicitudPlantilla { get; private set; }
    public long IdPlantillaEtapa { get; private set; }
    public bool RevAprobado { get; private set; }
    public DateTime? FechaRevision { get; private set; }
    #endregion

    private readonly List<HistorialRevision> _historialRevision = [];
    public IReadOnlyCollection<HistorialRevision> HistRevision => _historialRevision;

    #region Relaciones
    public FirmanteFlujo FirmanteFlujo { get; private set; } = default!;
    public SolicitudPlantilla SolicitudPlantilla { get; private set; } = default!;

    #endregion

    #region Propiedades de la Plantilla Etapa, para simular el runtime y no depender de el agregado de plantilla
    public long? OrdenEtapa { get; private set; }
    public long IdUsuarioAsignado { get; private set; }
    public bool EsFirmante { get; private set; }
    public bool EsVisador { get; private set; }
    public bool EsNuevo { get; private set; } = false;

    #endregion   

    #region Constructor
    private SolicitudFirmanteFlujo() { } // EF

    private SolicitudFirmanteFlujo(
      long idSolicitudFirmanteFlujo,
      long idFirmanteFlujo,
      long idSolicitudPlantilla,
      long idPlantillaEtapa,
      long ordenEtapa,
      bool revAprobado,
      long idUsuarioAsignado)
    {
        Id = idSolicitudFirmanteFlujo;
        IdFirmanteFlujo = idFirmanteFlujo;
        IdSolicitudPlantilla = idSolicitudPlantilla;
        IdPlantillaEtapa = idPlantillaEtapa;
        RevAprobado = revAprobado;
        OrdenEtapa = ordenEtapa;
        IdUsuarioAsignado = idUsuarioAsignado;
        Estado = true;
    }

    public static SolicitudFirmanteFlujo Crear(
          long idSolicitudFirmanteFlujo,
          long idFirmanteFlujo,
          long idSolicitudPlantilla,
          long idPlantillaEtapa,
          long ordenEtapa,
          long idUsuarioAsignado)
    {

        return new SolicitudFirmanteFlujo
        (
            idSolicitudFirmanteFlujo, idFirmanteFlujo, idSolicitudPlantilla, idPlantillaEtapa, ordenEtapa, false, idUsuarioAsignado
        );
    }

    #endregion

    #region Metodos
    public void AsignarSolicitudPlantilla(long idSolicitudPlantilla)
    {
        IdSolicitudPlantilla = idSolicitudPlantilla;
    }

    public void AsignarDatosEtapa(
        long? ordenEtapa,
        long idUsuarioAsignado,
        bool esFirmante,
        bool esVisador)
    {
        OrdenEtapa = ordenEtapa;
        IdUsuarioAsignado = idUsuarioAsignado;
        EsFirmante = esFirmante;
        EsVisador = esVisador;
    }

    public void Aprobar()
    {
        if (RevAprobado)
            throw new DomainException(SolicitudErrors.FirmanteAprobado);

        RevAprobado = true;
        FechaRevision = HoraSistema.Ahora;
    }

    public void ActualizarDatosSolicitudFirmante(
        long idFirmanteFlujo,
        long idSolicitudPlantilla,
        long idPlantillaEtapa,
        long? ordenEtapa)
    {
        IdFirmanteFlujo = idFirmanteFlujo;
        IdSolicitudPlantilla = idSolicitudPlantilla;
        IdPlantillaEtapa = idPlantillaEtapa;
        OrdenEtapa = ordenEtapa;
    }

    public void Inactivar()
    {
        Estado = false;
    }

    public void ValidarModificaionFirmante(SolicitudFirmanteFlujo request)
    {
        if (RevAprobado)
            throw new DomainException(SolicitudErrors.FirmanteModificacionNoValida);

        IdUsuarioAsignado = request.IdUsuarioAsignado;
    }

    public void ResetearPorRechazo()
    {
        RevAprobado = false;
        FechaRevision = null;
    }

    public void RestaurarAprobacion(DateTime? fechaRevision)
    {
        RevAprobado = true;
        FechaRevision = fechaRevision;
    }

    public HistorialRevision? ObtenerUltimoHistorialRevision()
    {
        return _historialRevision
            .Where(h => h.Estado)
            .OrderByDescending(h => h.FechaRevision)
            .ThenByDescending(h => h.Id)
            .FirstOrDefault();
    }

    public HistorialRevision? ObtenerHistorialHasta(DateTime fechaPunto)
    {
        return _historialRevision
            .Where(h => h.FechaRevision.HasValue && h.FechaRevision.Value <= fechaPunto)
            .OrderByDescending(h => h.FechaRevision)
            .ThenByDescending(h => h.Id)
            .FirstOrDefault();
    }

    public DateTime? ObtenerFechaRevisionEfectiva()
    {
        if (FechaRevision.HasValue)
            return FechaRevision;

        return _historialRevision
            .Where(h => h.Estado && h.FechaRevision.HasValue)
            .OrderByDescending(h => h.FechaRevision)
            .Select(h => h.FechaRevision)
            .FirstOrDefault();
    }

    public bool EsMismoOPosteriorA(SolicitudFirmanteFlujo punto)
        => Id == punto.Id || EsPosteriorA(punto);

    public bool EsPosteriorA(SolicitudFirmanteFlujo punto)
    {
        if (Id == punto.Id)
            return false;

        var fecha = ObtenerFechaRevisionEfectiva();
        var fechaPunto = punto.ObtenerFechaRevisionEfectiva();

        return (fecha.HasValue && fechaPunto.HasValue && fecha.Value > fechaPunto.Value)
            || OrdenEtapa > punto.OrdenEtapa;
    }

    public void InactivarHistorialRevisionPosterior(DateTime fechaPunto, bool incluirPuntoSeleccionado = false)
    {
        foreach (var historial in _historialRevision.Where(h => h.Estado))
        {
            if (!historial.FechaRevision.HasValue)
            {
                historial.Inactivar();
                continue;
            }

            var esPosterior = historial.FechaRevision.Value > fechaPunto;
            var esElPunto = incluirPuntoSeleccionado && historial.FechaRevision.Value >= fechaPunto;
            var esRechazoDesdeElPunto = !historial.RevAprobado && historial.FechaRevision.Value >= fechaPunto;

            if (esPosterior || esElPunto || esRechazoDesdeElPunto)
                historial.Inactivar();
        }
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    public void AgregarHistorialRevision(long idHistorialRevision, bool RevAprobado, DateTime? FechaRevision, int ciclo)
    {
        var historialRevision = HistorialRevision.Crear(
            idHistorialRevision,
            this.Id,
            this.IdFirmanteFlujo,
            this.IdSolicitudPlantilla,
            this.IdPlantillaEtapa,
            RevAprobado,
            FechaRevision,
            ciclo
        );

        historialRevision.MarcarComoNuevo();

        _historialRevision.Add(historialRevision);
    }

    #endregion
}
