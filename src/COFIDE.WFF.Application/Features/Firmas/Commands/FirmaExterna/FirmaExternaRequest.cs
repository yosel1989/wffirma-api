using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;

public sealed record FirmaExternaRequest
(
    long IdSolicitudDocumento, 
    IFormFile Documento
);