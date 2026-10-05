using COFIDE.WFF.Application.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.Services;

public class SolicitudSustentoService(IFileStorageService fileStorageService) : ISolicitudSustentoService
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    public async Task<IReadOnlyCollection<SustentoProcesadoInput>> CrearDocumentoSustentoInput(List<ArchivoEntrada>? archivos, string codigoSolicitud, CancellationToken cancellationToken)
    {
        if (archivos == null || archivos.Count == 0)
            return [];

        var lista = new List<SustentoProcesadoInput>();

        foreach (var file in archivos)
        {
            await using var stream = file.AbrirLectura();

            lista.Add(await ProcesarArchivoAsync(
                stream,
                file.Nombre,
                codigoSolicitud,
                cancellationToken));
        }

        return lista;
    }

    public async Task<IReadOnlyCollection<SustentoProcesadoInput>> CrearDocumentoSustentoInputDesdeStorage(IReadOnlyCollection<SolicitudSustento> sustentos, List<long>? idssustentosConservar,
        string codigoSolicitud, CancellationToken cancellationToken)
    {
        idssustentosConservar ??= [];

        var lista = new List<SustentoProcesadoInput>();

        foreach (var doc in sustentos
            .Where(x => x.Estado && idssustentosConservar.Contains(x.Id)))
        {
            string fileName = $"{doc.NombreSustento}.{doc.ExtensionSustento}";
            string path = _fileStorageService.CombinarRuta(doc.RutaSustento, fileName);

            await using var stream =
                await _fileStorageService.GetFileAsync(path, cancellationToken);

            lista.Add(await ProcesarArchivoAsync(
                stream,
                fileName,
                codigoSolicitud,
                cancellationToken));
        }

        return lista;
    }


    #region METODOS PRIVADOS

    private async Task<SustentoProcesadoInput> ProcesarArchivoAsync(Stream sourceStream, string fileName, string codigoSolicitud, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await sourceStream.CopyToAsync(ms, cancellationToken);

        var fileBytes = ms.ToArray();

        if (fileBytes.Length == 0)
            throw new Exception("Documento vacío");

        string subPath = _fileStorageService.ObtenerCarpetaSustentos(codigoSolicitud);

        string nombreSustento = Path.GetFileNameWithoutExtension(fileName);
        string extensionSustento = Path.GetExtension(fileName).TrimStart('.');

        using var uploadStream = new MemoryStream(fileBytes);

        await _fileStorageService.SaveFileAsync(
            uploadStream,
            fileName,
            subPath,
            fileName,
            cancellationToken);


        return new SustentoProcesadoInput
        {
            NombreSustento = nombreSustento,
            ExtensionSustento = extensionSustento,
            RutaSustento = subPath,
            PesoSustento = fileBytes.LongLength
        };

    }

    #endregion
}
