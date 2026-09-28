using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
namespace COFIDE.WFF.Application.Ports.Out;

public interface IZipStreamingService
{
    Task StreamZipAsync(
        Stream output,
        IReadOnlyCollection<ArchivoDescargaDto> archivos,
        CancellationToken cancellationToken);

    Task StreamZipAsync(
        Stream output,
        IReadOnlyCollection<FirmaZipRequestDto> archivos,
        CancellationToken cancellationToken);
}
