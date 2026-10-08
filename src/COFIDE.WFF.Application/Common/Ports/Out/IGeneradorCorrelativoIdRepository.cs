namespace COFIDE.WFF.Application.Common.Ports.Out;

public interface IGeneradorCorrelativoIdRepository
{
    #region PLANTILLA - ETAPA - FIRMANTES
    Task<long> ObtenerSiguienteIdPlantillaAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdEtapaAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdFirmanteAsync(CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdEtapaAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdFirmanteAsync(long cantidad, CancellationToken cancellationToken);
    #endregion

    #region SOLICITUD
    Task<long> ObtenerSiguienteIdSolicitudAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdMovimientoSolicitudAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdComentarioAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdSolicitudPlantillaAsync(CancellationToken cancellationToken);
    Task<long> ObtenerSiguienteIdSolitanteFirmanteAsync(CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdSolitanteFirmanteAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdDetalleFirmaAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdCodigosReferenciaAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdCorreosAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdDocumentoAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdSustentoAsync(long cantidad, CancellationToken cancellationToken);
    Task<List<long>> ObtenerBloqueSiguienteIdDocumentoTagAsync(long cantidad, CancellationToken cancellationToken);
    #endregion

    #region CODIGO SOLICITUD
    Task<long> ObtenerSiguienteCodigoSolicitudCorrelativoAsync(
    string codArea,
    string codTipoSolicitud,
    int year,
    CancellationToken cancellationToken);

    #endregion
}
