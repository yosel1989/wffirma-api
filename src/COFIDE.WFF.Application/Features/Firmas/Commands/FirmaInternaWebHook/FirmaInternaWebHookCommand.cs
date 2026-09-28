using COFIDE.WFF.Application.Common.Commands;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.FirmaInternaWebHook;

public sealed record FirmaInternaWebHookCommand
(
    string Code,
    string? DocumentId,
    string Date,
    string Signer,
    string DniSigner
) : CreateCommand<string?>;