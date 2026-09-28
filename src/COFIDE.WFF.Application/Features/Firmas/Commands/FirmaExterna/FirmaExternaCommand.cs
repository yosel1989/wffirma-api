using COFIDE.WFF.Application.Common.Commands;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;

public sealed record FirmaExternaCommand
(
    long IdSolicitud,
    long IdSolicitudDocumento,
    IFormFile Documento
) : CreateCommand<string?>;