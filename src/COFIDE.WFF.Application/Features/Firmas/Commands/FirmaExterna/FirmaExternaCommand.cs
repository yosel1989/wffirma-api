using COFIDE.WFF.Application.Common.Commands;
using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaExterna;

public sealed record FirmaExternaCommand
(
    long IdSolicitud,
    long IdSolicitudDocumento,
    ArchivoEntrada Documento
) : CreateCommand<string?>;