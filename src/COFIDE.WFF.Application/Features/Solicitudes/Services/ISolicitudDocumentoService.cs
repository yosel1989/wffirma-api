using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public interface ISolicitudDocumentoService
{
    Task<IReadOnlyCollection<DocumentoProcesadoInput>> CrearDocumentoInput(List<IFormFile>? archivos, string codigoSolicitud, bool inCodigo, string? documentosTag, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DocumentoProcesadoInput>> CrearDocumentoInputDesdeStorage(IReadOnlyCollection<SolicitudDocumento> documentos, List<long>? idsDocumentosConservar,
        string codigoSolicitud, bool inCodigo, CancellationToken cancellationToken);
    Task SincronizarCambioCodigoAsync(Solicitud solicitud, bool nuevoInCodigo, CancellationToken cancellationToken);
    Task SincronizarTagDocumentoAsync(Solicitud solicitud, CancellationToken cancellationToken);
}
