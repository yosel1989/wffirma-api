using COFIDE.WFF.Application.Common.Commands;

namespace COFIDE.WFF.Application.Features.Firmas.Commands.RestablecerDocumentoFirmado;
public sealed record RestablecerDocumentoFirmadoCommand
(
    List<long> IdDetalleFirma
) : CreateCommand<string?>;

