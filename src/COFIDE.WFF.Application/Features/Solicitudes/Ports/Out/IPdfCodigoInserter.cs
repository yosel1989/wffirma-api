namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
public interface IPdfCodigoInserter
{
    /// <summary>
    /// Inserta un código en la cabecera de la primera página del PDF.
    /// </summary>
    /// <param name="inputPdf">PDF original en stream</param>
    /// <param name="outputPdf">PDF resultante en stream</param>
    /// <param name="codigoDocumento">Código a insertar</param>
    byte[] InsertCodigo(byte[] inputPdfBytes, string codigoDocumento);

    byte[] InsertTag(
       byte[] inputPdfBytes,
       string tag,
       int pagina,
       double x,
       double y,
       string colorFuente,
       decimal firmaMargenAlto,
       CancellationToken cancellationToken = default);

    Task<Stream> InsertTagAsync(
       Stream inputPdf,
       string tag,
       int pagina,
       double x,
       double y,
       string colorFuente,
       decimal firmaMargenAlto,
       CancellationToken cancellationToken = default);
}
