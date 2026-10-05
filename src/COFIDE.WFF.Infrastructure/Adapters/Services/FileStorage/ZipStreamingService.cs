using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Application.Features.Publico.DescargaDocumento;
using COFIDE.WFF.Application.Ports.Out;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
namespace COFIDE.WFF.Infrastructure.Adapters.Services.FileStorage;

public sealed class ZipStreamingService(
        IFileStorageService fileStorageService,
        ILogger<ZipStreamingService> logger
    ) : IZipStreamingService
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<ZipStreamingService> _logger = logger;

    public async Task StreamZipAsync(
        Stream output,
        IReadOnlyCollection<ArchivoDescargaDto> archivos,
        CancellationToken cancellationToken)
    {
        try
        {
            var nombresUsados = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            using var zipBuffer = new MemoryStream();

            using (var archive = new ZipArchive(
                zipBuffer,
                ZipArchiveMode.Create,
                leaveOpen: true))
            {
                foreach (var archivo in archivos)
                {
                    var nombreZip = GenerarNombreUnico(
                        archivo.NombreArchivo,
                        nombresUsados);

                    var entry = archive.CreateEntry(
                        nombreZip,
                        CompressionLevel.Fastest);

                    await using var entryStream = entry.Open();

                    await using var fileStream =
                        await _fileStorageService.GetFileAsync(
                            archivo.RutaFisica,
                            cancellationToken);

                    await fileStream.CopyToAsync(
                        entryStream,
                        cancellationToken);
                }
            }

            zipBuffer.Position = 0;

            await zipBuffer.CopyToAsync(output, cancellationToken);

            await output.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando ZIP para descarga");
        }
    }        

    public async Task StreamZipAsync(
        Stream output,
        IReadOnlyCollection<FirmaZipRequestDto> archivos,
        CancellationToken cancellationToken)
    {

        using var archive =
           new ZipArchive(
               output,
               ZipArchiveMode.Create,
               leaveOpen: true);

        foreach (var archivo in archivos)
        {
            var entry =
                archive.CreateEntry(
                    archivo.FileName,
                    CompressionLevel.Fastest);

            await using var entryStream =
                entry.Open();

            archivo.Content.Position = 0;

            await archivo.Content.CopyToAsync(
                entryStream,
                cancellationToken);
        }
    }


    public async Task<byte[]> GenerarZipAsync(
        IReadOnlyCollection<EntradaZip> entradas,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();

        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var entrada in entradas)
            {
                await using var origen = await _fileStorageService.GetFileAsync(entrada.RutaArchivo, cancellationToken);

                var entry = archive.CreateEntry(entrada.NombreEnZip, CompressionLevel.Fastest);
                await using var destino = entry.Open();
                await origen.CopyToAsync(destino, cancellationToken);
            }
        }

        return buffer.ToArray();
    }

    private static string GenerarNombreUnico(
        string nombreOriginal,
        HashSet<string> nombresUsados)
    {
        var nombreLimpio = Path.GetFileName(nombreOriginal);

        if (nombresUsados.Add(nombreLimpio))
            return nombreLimpio;

        var nombreBase = Path.GetFileNameWithoutExtension(nombreLimpio);
        var extension = Path.GetExtension(nombreLimpio);

        var contador = 2;

        while (true)
        {
            var candidato = $"{nombreBase} ({contador}){extension}";

            if (nombresUsados.Add(candidato))
                return candidato;

            contador++;
        }
    }

}
