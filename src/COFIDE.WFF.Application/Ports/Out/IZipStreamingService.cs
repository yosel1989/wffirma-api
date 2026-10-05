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

    /// <summary>
    /// Genera un ZIP en memoria con los archivos indicados. Los nombres dentro del ZIP
    /// se usan tal como vienen (el llamador decide carpetas internas y nombres únicos).
    /// </summary>
    Task<byte[]> GenerarZipAsync(
        IReadOnlyCollection<EntradaZip> entradas,
        CancellationToken cancellationToken);
}
