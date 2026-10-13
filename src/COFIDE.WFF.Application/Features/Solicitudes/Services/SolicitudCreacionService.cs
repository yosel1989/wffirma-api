using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Domain.Common.Tiempo;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudCreacionService(
        IMaestroRepository maestroRepository,
        IGeneradorCorrelativoIdRepository generadorCorrelativoIdRepository
    ) : ISolicitudCreacionService
{
    private readonly IMaestroRepository _maestroRepository = maestroRepository;
    private readonly IGeneradorCorrelativoIdRepository _generadorCorrelativoIdRepository = generadorCorrelativoIdRepository;

    public async Task<Solicitud> CrearSolicitudBase(
        SolicitudRequestDto request,
        string codigoArea,
        long idUSuario,
        long idArea,
        long idAreaPadre,
        CancellationToken cancellationToken)
    {
        var idSolicitud = await _generadorCorrelativoIdRepository.ObtenerSiguienteIdSolicitudAsync(cancellationToken);

        #region 01. Traer la información de Código
        var fechaActual = HoraSistema.Ahora;
        var anio = fechaActual.Year;
        var tipoSolicitud = await _maestroRepository.GetTipoSolicitudByIdAsync(request.IdTipoSolicitud, cancellationToken, true);
        var codigoTipoSolicitud = tipoSolicitud == null ? "NN" : tipoSolicitud.DescripcionCorta;
        var correlativoCodigoSolicitud = await _generadorCorrelativoIdRepository.ObtenerSiguienteCodigoSolicitudCorrelativoAsync(codigoArea, codigoTipoSolicitud, anio, cancellationToken);
        #endregion       

        var input = MapearInputSolicitud(request, anio, codigoArea, codigoTipoSolicitud, correlativoCodigoSolicitud);
        var solicitudBase = SolicitudFactory.CrearSolicitudBase(input, idSolicitud, idUSuario, idArea, idAreaPadre);
        return solicitudBase;
    }


    private static CrearSolicitudInput MapearInputSolicitud(SolicitudRequestDto request, int anio, string codigoArea, string codigoTipoSolicitud, long correlativoCodigoSolicitud)
    {
        return new CrearSolicitudInput
        {
            IdTipoSolicitud = request.IdTipoSolicitud,
            IdSubTipoSolicitud = request.IdSubTipoSolicitud,
            IdTipoOperacion = request.IdtipoOperacion,

            Asunto = request.Asunto?.Trim(),
            Mensaje = request.Mensaje?.Trim(),

            EsUrgente = request.EsUrgente,
            InCodigo = request.InCodigo,

            FechaLimite = request.FechaLimite,
            PesoTotalDocu = request.PesoTotalDocu,

            CodigosReferencia = request.CodigosReferencia?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>(),

            Correos = request.Correos?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>(),

            Anio = anio,
            CodigoArea = codigoArea,
            CodigoTipoSolicitud = codigoTipoSolicitud,
            CorrelativoCodigoSolicitud = correlativoCodigoSolicitud
        };
    }
}
