using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
public class PlantillaEtapa : AuditableEntity<long>
{

    #region 01 Campos
    public Plantilla Plantilla { get; set; } = null!;
    public long IdPlantilla { get; private set; }
    public long OrdenEtapa { get; private set; }
    public Vo.NombreEtapa NombreEtapa { get; private set; } = default!;
    public Vo.TipoEtapa TipoEtapa { get; private set; } = default!;
    public long IdTipoEtapa { get; private set; }
    public Vo.TipoAprobador TipoAprobador { get; private set; } = default!;
    public long IdTipoAprobador { get; private set; }
    #endregion

    public ParametroDetalle ParametroTipoEtapa { get; set; } = default!;
    public ParametroDetalle ParametroTipoAprobador { get; set; } = default!;

    #region Propiedades de la Plantilla Etapa, para los eventos no serán mapeados en BD
    public bool EsNuevo { get; private set; } = false;
    public Guid? TempId { get; private set; }
    #endregion

    #region 02. Hijos
    private readonly List<FirmanteFlujo> _firmantes = new();
    public IReadOnlyCollection<FirmanteFlujo> Firmantes => _firmantes;
    #endregion

    #region 03. Constructor
    private PlantillaEtapa() { }

    private PlantillaEtapa(
        long idPlantilla,
        long idPlantillaEtapa,
        long ordenEtapa,
        Vo.NombreEtapa nombreEtapa,
        Vo.TipoEtapa tipoEtapa,
        long idTipoEtapa,
        Vo.TipoAprobador tipoAprobador,
        long idTipoAprobador)
    {
        Id = idPlantillaEtapa;
        IdPlantilla = idPlantilla;
        OrdenEtapa = ordenEtapa;
        NombreEtapa = nombreEtapa;
        TipoEtapa = tipoEtapa;
        IdTipoEtapa = idTipoEtapa;
        TipoAprobador = tipoAprobador;
        IdTipoAprobador = idTipoAprobador;
        Estado = true;
    }
    #endregion

    #region Creacion
    public static PlantillaEtapa Crear(
            long idPlantilla,
            long idPlantillaEtapa,
            long ordenEtapa,
            string nombreEtapa,
            Vo.TipoEtapa tipoEtapa,
            long idTipoEtapa,
            Vo.TipoAprobador tipoAprobador,
            long idTipoAprobador)
    {
        if (idPlantilla <= 0)
            throw new DomainException(PlantillaErrors.IdPlantillaInvalido);

        if (idPlantillaEtapa <= 0)
            throw new DomainException(PlantillaErrors.IdPlantillaEtapaInvalido);

        if (ordenEtapa <= 0)
            throw new DomainException(PlantillaErrors.OrdenEtapaInvalido);


        Vo.NombreEtapa nombreEtapaVO = Vo.NombreEtapa.Crear(nombreEtapa);

        return new PlantillaEtapa
        (
            idPlantilla, idPlantillaEtapa, ordenEtapa, nombreEtapaVO, tipoEtapa, idTipoEtapa, tipoAprobador, idTipoAprobador
        );
    }

    public void SetTempId(Guid tempId)
    {
        TempId = tempId;
    }
    #endregion

    #region Edición
    public void Editar(
        long orden,
        string nombre,
        Vo.TipoEtapa tipoEtapa,
        long idTipoEtapa,
        Vo.TipoAprobador tipoAprobador,
        long idTipoAprobador)
    {
        OrdenEtapa = orden;
        NombreEtapa.Actualizar(nombre);
        TipoEtapa = tipoEtapa;
        IdTipoEtapa = idTipoEtapa;
        TipoAprobador = tipoAprobador;
        IdTipoAprobador = idTipoAprobador;
    }
    #endregion

    public void AsignarTipoEtapa(Vo.TipoEtapa tipoEtapa)
    {
        TipoEtapa = tipoEtapa;
    }

    public void AgregarFirmante(
         long idFirmanteFlujo,
         long idUsuarioAsignado,
         string? cargoUsuario,
         string? areaUsuario,
         string? gerenciaUsuario,
         long idUsuarioOrigen)
    {
        var firmante = FirmanteFlujo.Crear(
            this.Id,
            idFirmanteFlujo,
            idUsuarioAsignado,
            cargoUsuario,
            areaUsuario,
            gerenciaUsuario,
            idUsuarioOrigen
        );

        firmante.MarcarComoNuevo();

        _firmantes.Add(firmante);
    }

    public void EditarFirmantes(List<FirmanteEditModel> firmantesRequest, Func<long> generarIdFirmante, long usuarioActualId)
    {
        var idsRequest = firmantesRequest.Where(x => x.Id != 0).Select(f => f.Id).ToHashSet();

        foreach (var firmante in _firmantes.Where(f => f.Estado && !idsRequest.Contains(f.Id)))
        {
            if (firmante.IdUsuarioAsignado == usuarioActualId)
                throw new DomainException(PlantillaErrors.EliminarPropioFirmante);

            firmante.Desactivar();
        }

        foreach (var fReq in firmantesRequest)
        {
            var existente = _firmantes.FirstOrDefault(f => f.Id == fReq.Id && f.Estado);

            if (existente is null)
            {
                var nuevo = FirmanteFlujo.Crear(
                    this.Id,
                    generarIdFirmante(),
                    fReq.IdUsuarioAsignado,
                    fReq.CargoUsuario,
                    fReq.AreaUsuario,
                    fReq.GerenciaUsuario,
                    fReq.IdUsuarioOrigen
                );

                nuevo.MarcarComoNuevo();

                _firmantes.Add(nuevo);
            }
            else
            {
                existente.Editar(
                    fReq.CargoUsuario,
                    fReq.AreaUsuario,
                    fReq.GerenciaUsuario
                );
            }
        }

        ValidarTotalFirmantePorEtapa();
    }

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    public void Inactivar()
    {
        Estado = false;

        foreach (var firmante in _firmantes.Where(f => f.Estado))
        {
            firmante.Desactivar();
        }
    }

    #region Validaciones 

    public void ValidarTotalFirmantePorEtapa3()
    {
        TipoEtapa.ValidarCantidadFirmantes(_firmantes.Count(f => f.Estado));
    }

    public void ValidarTotalFirmantePorEtapa(Vo.TipoEtapa? tipoEtapa = null)
    {
        var firmantesActivos = _firmantes.Where(f => f.Estado).ToList();

        if (firmantesActivos.Count == 0)
            throw new DomainException(PlantillaErrors.ValidezNuevaEtapaFirmante);

        if (tipoEtapa == null)
            TipoEtapa.ValidarCantidadFirmantes(firmantesActivos.Count);
        else
            tipoEtapa.ValidarCantidadFirmantes(firmantesActivos.Count);
    }
    #endregion
}