using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.ActivarDocumentoBloqueado;

public sealed record ActivarDocumentoBloqueadoCommand(
    long IdDocumento
) : CreateCommand<string>;