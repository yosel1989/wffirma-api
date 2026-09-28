using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;

public class SolicitudPlantilla : AuditableEntity<long>
{
    public long IdPlantilla { get; private set; }
    public long IdSolicitud { get; }
    public Solicitud Solicitud { get; private set; } = default!;
    public Plantilla Plantilla { get; private set; } = default!;

    private readonly List<SolicitudFirmanteFlujo> _firmantesFlujo = [];
    public IReadOnlyCollection<SolicitudFirmanteFlujo> FirmantesFlujo => _firmantesFlujo;

    #region Propiedades de la Solicitud Plantilla, no considerarse en la persistencia
    public bool EsNuevo { get; private set; } = false;

    #endregion   

    private SolicitudPlantilla() { } // EF
    private SolicitudPlantilla(long idSolicitudPlantilla, Solicitud solicitud, Plantilla plantilla)
    {
        Id = idSolicitudPlantilla;
        Solicitud = solicitud;
        Plantilla = plantilla;
        Estado = true;
    }

    public static SolicitudPlantilla Crear(long idSolicitudPlantilla, Solicitud solicitud, Plantilla plantilla)
    {
        if (plantilla is null)
            throw new DomainException(PlantillaErrors.IdPlantillaInvalido);

        var solicitudPantilla = new SolicitudPlantilla(idSolicitudPlantilla, solicitud, plantilla);

        solicitudPantilla.MarcarComoNuevo();

        return solicitudPantilla;
    }

    public void AgregarFirmanteFlujo(
       long idSolicitudFirmanteFlujo,
       long idFirmanteFlujo,
       long idPlantillaEtapa,
       long ordenEtapa,
       long idUsuarioAsignado)
    {
        var firmanteFlujo = SolicitudFirmanteFlujo.Crear(
            idSolicitudFirmanteFlujo,
            idFirmanteFlujo,
            this.Id,
            idPlantillaEtapa,
            ordenEtapa,
            idUsuarioAsignado
        );

        firmanteFlujo.MarcarComoNuevo();

        _firmantesFlujo.Add(firmanteFlujo);
    }

    public void Inactivar()
    {
        Estado = false;

        foreach (var firmante in _firmantesFlujo.Where(x => x.Estado))
        {
            firmante.Inactivar();
        }
    }

    #region Metodos

    public bool FlujoEstaCompleto()
    {
        return _firmantesFlujo.Where(f => f.Estado).All(f => f.RevAprobado && f.Estado);
    }

    public SolicitudFirmanteFlujo? BuscarFirmantePendientes(long usuarioAccionId)
    {
        return _firmantesFlujo
            .FirstOrDefault(f => f.IdUsuarioAsignado == usuarioAccionId && f.Estado && !f.RevAprobado);
    }

    public SolicitudFirmanteFlujo? BuscarFirmantes(long usuarioAccionId)
    {
        return _firmantesFlujo
            .FirstOrDefault(f => f.IdUsuarioAsignado == usuarioAccionId && f.Estado);
    }


    public SolicitudFirmanteFlujo ObtenerFirmanteActivo(long usuarioAccionId)
    {    
        var firmante = BuscarFirmantePendientes(usuarioAccionId);

        if (firmante == null)
            throw new DomainException(SolicitudErrors.NoPuedeAprobarSolicitud);

        return firmante;
    }

    public void ActualizarPlantilla(long idPlantilla)
    {
        IdPlantilla = idPlantilla;
    }

    public void ResetearFirmantes()
    {
        foreach (var firmante in _firmantesFlujo.Where(x => x.Estado))
        {
            firmante.ResetearPorRechazo();
        }
    }

    public void DesactivarFirmanteFlujo(SolicitudFirmanteFlujo firmante)
    {
        if (firmante is null)
            throw new DomainException(SolicitudErrors.FirmanteNotFound);

        if (!_firmantesFlujo.Contains(firmante))
            return;

        if (firmante.RevAprobado)
            throw new DomainException(SolicitudErrors.FirmanteEliminacionNoValida);

        firmante.Desactivar();
    }

    public bool EtapaEstaCerrada()
    {
        return _firmantesFlujo.Any(f => f.RevAprobado);
    }

    public long ObtenerIdPlantillaEtapaActualPorUsuarioAsignado(long idFirmanteFlujo)
    {
        var resultado = _firmantesFlujo.FirstOrDefault(f => f.IdUsuarioAsignado == idFirmanteFlujo && f.Estado);
        return resultado!.IdPlantillaEtapa;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    #endregion
}