using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;

public class Plantilla : AuditableEntity<long>
{
    #region Campos de la Plantilla
    public Vo.NombrePlantilla? NombrePlantilla { get; private set; } = default!;
    public Vo.DescripcionPlantilla? Descripcion { get; private set; } = default!;
    public bool? Visibilidad { get; private set; }
    public long? IdUsuarioPropietario { get; private set; }
    public string? GerenciaUsuario { get; private set; }
    public bool? EsReutilizable { get; private set; }
    public long? IdAreaPlantilla { get; private set; }
    public long? IdAreaPadrePlantilla { get; private set; }
    #endregion

    #region Referencias
    private readonly List<PlantillaEtapa> _etapas = new();
    public IReadOnlyCollection<PlantillaEtapa> Etapas => _etapas;
    #endregion

    #region Propiedades de la Plantilla, para los eventos no serán mapeados en BD
    public bool EsNuevo { get; private set; } = false;
    #endregion

    #region Constructor
    private Plantilla() { }

    private Plantilla(
        long idPlantilla,
        Vo.NombrePlantilla? nombrePlantilla,
        Vo.DescripcionPlantilla? descripcion,
        bool? visibilidad,
        long? idUsuarioPropietario,
        string? gerenciaUsuario,
        bool? esReutilizable)
    {
        Id = idPlantilla;
        NombrePlantilla = nombrePlantilla;
        Descripcion = descripcion;
        Visibilidad = visibilidad;
        IdUsuarioPropietario = idUsuarioPropietario;
        GerenciaUsuario = gerenciaUsuario;
        EsReutilizable = esReutilizable;
        Estado = true;
    }
    #endregion

    #region Creación de Plantilla
    public static Plantilla Crear(
           long idPlantilla,
           string? nombrePlantilla,
           string? descripcion,
           bool? visibilidad,
           long? idUsuarioPropietario,
           string? gerenciaUsuario,
           bool? esReutilizable)
    {
        if (idPlantilla <= 0)
            throw new DomainException(PlantillaErrors.IdPlantillaInvalido);

        if ((visibilidad.HasValue && visibilidad!.Value) &&
            ((esReutilizable.HasValue && !esReutilizable!.Value) || !esReutilizable.HasValue))
            throw new DomainException(PlantillaErrors.ValidarPlantillaPublicaReutilizable);

        Vo.NombrePlantilla? nombrePlantillaVO = Vo.NombrePlantilla.Crear(nombrePlantilla);
        Vo.DescripcionPlantilla? descripcionVO = Vo.DescripcionPlantilla.Crear(descripcion);

        return new Plantilla
        (
            idPlantilla, nombrePlantillaVO, descripcionVO, visibilidad, idUsuarioPropietario, gerenciaUsuario, esReutilizable
        );
    }
    #endregion

    #region Editar Flujo

    public void EditarFlujo(
       PlantillaEditModel plantillaRequest,
       Func<long> generarIdEtapa, Func<long> generarIdFirmante,
       Dictionary<long, ParametroDetalle> tipoEtapaDict,
       Dictionary<long, ParametroDetalle> tipoAprobadorDict,
       long usuarioActualId)
    {
        var etapasIdsRequest = plantillaRequest.EtapasRequest.Where(x => x.Id != 0).Select(e => e.Id).ToHashSet();

        foreach (var etapa in _etapas.Where(e => e.Estado && !etapasIdsRequest.Contains(e.Id)))
        {
            etapa.Inactivar();
        }

        foreach (var etapaReq in plantillaRequest.EtapasRequest)
        {
            var tipoEtapa = Vo.TipoEtapa.ObtenerTipoEtapa(etapaReq.IdTipoEtapa, tipoEtapaDict);
            var tipoAprobador = Vo.TipoAprobador.ObtenerTipoAprobador(etapaReq.IdTipoAprobador, tipoAprobadorDict);
            var etapaExistente = _etapas.FirstOrDefault(e => e.Id == etapaReq.Id && e.Estado);

            if (etapaExistente is null)
            {
                var nueva = AgregarEtapas(
                    generarIdEtapa(),
                    etapaReq.Orden,
                    etapaReq.Nombre,
                    tipoEtapa,
                    etapaReq.IdTipoEtapa,
                    tipoAprobador,
                    etapaReq.IdTipoAprobador
                );

                nueva.MarcarComoNuevo();
                nueva.SetTempId(etapaReq.TempId);
                nueva.EditarFirmantes(etapaReq.Firmantes, generarIdFirmante, usuarioActualId);
            }
            else
            {
                etapaExistente.Editar(
                    etapaReq.Orden,
                    etapaReq.Nombre,
                    tipoEtapa,
                    etapaReq.IdTipoEtapa,
                    tipoAprobador,
                    etapaReq.IdTipoAprobador
                );

                etapaExistente.EditarFirmantes(etapaReq.Firmantes, generarIdFirmante, usuarioActualId);
            }
        }

        ValidarFlujoCompleto();
    }

    #endregion

    #region ClonarPlantilla


    #endregion

    #region Entidades Hijas
    public PlantillaEtapa AgregarEtapas(
        long idPlantillaEtapa,
        long ordenEtapa,
        string nombreEtapa,
        Vo.TipoEtapa tipoEtapa,
        long idTipoEtapa,
        Vo.TipoAprobador tipoAprobador,
        long idTipoAprobador)
    {
        var etapa = PlantillaEtapa.Crear(
                this.Id,
                idPlantillaEtapa,
                ordenEtapa,
                nombreEtapa,
                tipoEtapa,
                idTipoEtapa,
                tipoAprobador,
                idTipoAprobador);

        _etapas.Add(etapa);

        return etapa;
    }
    #endregion

    #region Validaciones

    public void ValidarTotalFirmantes()
    {
        long total_permitido = 20;
        var total = _etapas.Where(e => e.Estado).SelectMany(e => e.Firmantes).Count();

        if (total > total_permitido)
            throw new DomainException(PlantillaErrors.CantidadMaximaAprobadores.Format(total_permitido));
    }

    public void ValidarFlujoCompleto()
    {
        if (!_etapas.Any(e => e.Estado))
            throw new DomainException(PlantillaErrors.ExistenciaPrimeraEtapa);

        foreach (var etapa in _etapas.Where(e => e.Estado))
        {
            etapa.ValidarTotalFirmantePorEtapa();
        }

        ValidarTotalFirmantes();
        ValidarOrdenEtapas();
    }

    private void ValidarOrdenEtapas()
    {
        var ordenes = _etapas
            .Where(e => e.Estado)
            .Select(e => e.OrdenEtapa)
            .OrderBy(x => x)
            .ToList();

        if (ordenes.Count == 0)
            return;

        if (ordenes.First() != 1)
            throw new DomainException(PlantillaErrors.SecuenciaInicioEtapasInvalida);

        for (int i = 0; i < ordenes.Count; i++)
        {
            if (ordenes[i] != i + 1)
                throw new DomainException(PlantillaErrors.SecuenciaEtapasInvalida);
        }
    }

    public void ValidarPuedeAgregarFirmanteEtapa(Vo.TipoEtapa tipoEtapa)
    {
        var ultima = _etapas
            .Where(e => e.Estado)
            .OrderByDescending(e => e.OrdenEtapa)
            .FirstOrDefault();

        if (ultima == null)
            return;

        ultima.ValidarTotalFirmantePorEtapa(tipoEtapa);
    }

    public bool EsPlantillaMaestra => EsReutilizable ?? false;

    public void MarcarComoNuevo()
    {
        EsNuevo = true;
    }

    public void PermitirActualizarPlantilla(long usuarioId, string gerenciaUsuario, Usuario usuario)
    {
        if (usuario.EsCoordinador())
        {   
            if ((Visibilidad.HasValue && Visibilidad.Value) && GerenciaUsuario != gerenciaUsuario)
            {
                throw new DomainException(PlantillaErrors.PermisoCoordinadorPlantillaArea);
            }
        }

        if (usuario.EsSolicitante())
        {
            if (IdUsuarioPropietario != usuarioId && (Visibilidad.HasValue && !Visibilidad.Value))
                throw new DomainException(PlantillaErrors.PropietarioPlantilla);
        }

    }

    public bool ValidarPermisoPlantilla(long usuarioId, string gerenciaUsuario, Usuario usuario)
    {
        if ((Visibilidad.HasValue && Visibilidad.Value))
        {
            if (usuario.EsCoordinador())
            {
                if (GerenciaUsuario != gerenciaUsuario)
                    return false;

            }

            if (usuario.EsSolicitante())
            {
                if (IdUsuarioPropietario != usuarioId)
                    return false;
            }
        }

        return true;
    }

    #endregion
}