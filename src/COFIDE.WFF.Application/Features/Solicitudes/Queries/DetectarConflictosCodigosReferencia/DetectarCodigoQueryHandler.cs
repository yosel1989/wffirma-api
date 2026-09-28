using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Domain.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.DetectarConflictosCodigosReferencia;

public sealed class DetectarCodigoQueryHandler(
        IMapper mapper,
        ISolicitudRepository solicitudRepository,
        IParametroRepository parametroRepository
    ) : BaseQueryHandler<DetectarCodigoQuery, DetectarConflictosResponse>(mapper)
{
    private readonly ISolicitudRepository _solicitudRepository = solicitudRepository;
    private readonly IParametroRepository _parametroRepository = parametroRepository;

    public override async Task<DetectarConflictosResponse> Handle(DetectarCodigoQuery request, CancellationToken cancellationToken)
    {
        if (request.CodigosReferencia == null || request.CodigosReferencia.Count == 0)
        {
            return new DetectarConflictosResponse(0, []);
        }

        var conflictos = new List<CodigoReferenciaConflictoDto>();
        var codigosValidos = new List<CodigoReferencia>();

        var parametrosCodReferencia = await _parametroRepository
        .ObtenerDetallePorCodigoParametro("COD_REFERENCIA", cancellationToken);

        var maxLongitudCodReferencia = parametrosCodReferencia
            .Select(x => int.TryParse(x.Valor, out var valor) ? valor : CodigoReferencia.MaxLongitud)
            .FirstOrDefault();

        if (maxLongitudCodReferencia <= 0)
        {
            maxLongitudCodReferencia = CodigoReferencia.MaxLongitud;
        }

        foreach (var codigo in request.CodigosReferencia)
        {
            try
            {
                var vo = CodigoReferencia.Crear(codigo, maxLongitudCodReferencia);
                codigosValidos.Add(vo);
            }
            catch (DomainExceptionBase ex)
            {
                conflictos.Add(
                    new CodigoReferenciaConflictoDto(
                        codigo ?? string.Empty,
                        ex.Message));
            }
        }

        // Duplicados en la misma lista
        var duplicados = codigosValidos
            .GroupBy(c => c.Valor)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        conflictos.AddRange(
            duplicados.Select(c =>
                new CodigoReferenciaConflictoDto(
                    c,
                    "Duplicidad")));

        var candidatosBd = codigosValidos
            .Select(c => c.Valor)
            .Distinct()
            .ToList();

        if (candidatosBd.Count > 0)
        {
            var existentes = await _solicitudRepository
                .ObtenerCodigosExistentesAsync(
                    request.IdSolicitud,
                    candidatosBd,
                    cancellationToken);

            conflictos.AddRange(
                existentes.Select(c =>
                    new CodigoReferenciaConflictoDto(
                        c.CodigoReferencia,
                        $"Código relacionado a la Solicitud \"{c.CodigoSolicitud}\"")));
        }

        return new DetectarConflictosResponse(
            conflictos.Count,
            conflictos);
    }
}
