using COFIDE.WFF.Application.Features.Firmas.DTOs;
namespace COFIDE.WFF.Application.Features.Firmas.Ports.Out;

public interface IFirmaPort
{
    Task<FirmaInternaResponseDto> FirmarDocumentoAsync(
      FirmarDocumentoRequestDto request,
      CancellationToken cancellationToken);

    Task<FirmaInternaResponseDto> FirmarBatchAsync(
            FirmarDocumentoRequestDto request,
            CancellationToken cancellationToken);

    Task<byte[]> ObtenerDocumentoFirmadoAsync(
          string codigoUsuario, string codigo, string documentId,
            CancellationToken cancellationToken);
}
