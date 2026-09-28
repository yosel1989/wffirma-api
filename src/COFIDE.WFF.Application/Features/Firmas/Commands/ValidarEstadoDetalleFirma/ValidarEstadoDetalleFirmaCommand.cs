using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.ValidarEstadoDetalleFirma;

public sealed record ValidarEstadoDetalleFirmaCommand
(
    long IdSolicitud,
    List<long> Documentos
) : CreateCommand<ValidarEstadoDetalleFirmaDto?>;
