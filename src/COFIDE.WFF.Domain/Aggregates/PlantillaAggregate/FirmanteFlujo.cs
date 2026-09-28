using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;

public class FirmanteFlujo : AuditableEntity<long>
{
    public long IdPlantillaEtapa { get; private set; }
    public long IdUsuarioAsignado { get; private set; }
    public string? CargoUsuario { get; private set; }
    public string? AreaUsuario { get; private set; }
    public string? GerenciaUsuario { get; private set; }
    public DateTime FechaAsignacion { get; private set; }
    public bool EsSuplente { get; private set; }
    public bool EsDelegado { get; private set; }
    public long IdUsuarioOrigen { get; set; }
    public bool EsNuevo { get; private set; } = false;
    public Usuario FirmanteFlujoUsuarioAsignado { get; set; } = default!;
    public PlantillaEtapa PlantillaEtapa { get; set; } = default!;

    private FirmanteFlujo() { } // EF

    private FirmanteFlujo(
      long idPlantillaEtapa,
      long idFirmanteFlujo,
      long idUsuarioAsignado,
      string? cargoUsuario,
      string? areaUsuario,
      string? gerenciaUsuario,
      long idUsuarioOrigen)
    {
        Id = idFirmanteFlujo;
        IdPlantillaEtapa = idPlantillaEtapa;
        IdUsuarioAsignado = idUsuarioAsignado;
        CargoUsuario = cargoUsuario;
        AreaUsuario = areaUsuario;
        GerenciaUsuario = gerenciaUsuario;
        FechaAsignacion = DateTime.Now;
        EsSuplente = false;
        EsDelegado = false;
        IdUsuarioOrigen = idUsuarioOrigen;
        Estado = true;
    }

    public static FirmanteFlujo Crear(
          long idPlantillaEtapa,
          long idFirmanteFlujo,
          long idUsuarioAsignado,
          string? cargoUsuario,
          string? areaUsuario,
          string? gerenciaUsuario,
          long idUsuarioOrigen)
    {
        if (idUsuarioAsignado <= 0)
            throw new DomainException(PlantillaErrors.UsuarioAsignadoInvalido);

        return new FirmanteFlujo
        (
            idPlantillaEtapa, idFirmanteFlujo, idUsuarioAsignado, cargoUsuario, areaUsuario, gerenciaUsuario, idUsuarioOrigen
        );
    }

    public void Editar(
        string? cargoUsuario,
        string? areaUsuario,
        string? gerenciaUsuario)
    {
        CargoUsuario = cargoUsuario;
        AreaUsuario = areaUsuario;
        GerenciaUsuario = gerenciaUsuario;
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }
}
