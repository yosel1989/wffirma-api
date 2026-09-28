using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudSustentoService
{
    Task<IReadOnlyCollection<SustentoProcesadoInput>> CrearDocumentoSustentoInput(List<IFormFile>? archivos, string codigoSolicitud, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SustentoProcesadoInput>> CrearDocumentoSustentoInputDesdeStorage(IReadOnlyCollection<SolicitudSustento> sustentos, List<long>? idssustentosConservar,
        string codigoSolicitud, CancellationToken cancellationToken);
}
