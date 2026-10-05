namespace COFIDE.WFF.Application.Common.Files;

/// <summary>
/// Archivo recibido por un caso de uso, independiente del mecanismo de transporte
/// (formulario HTTP, cola, Worker o prueba unitaria).
/// La Api lo construye a partir de IFormFile; Application nunca conoce IFormFile.
/// </summary>
/// <param name="Nombre">Nombre del archivo con extensión (sin rutas).</param>
/// <param name="ContentType">Tipo MIME informado por el cliente.</param>
/// <param name="Tamanio">Tamaño en bytes.</param>
/// <param name="AbrirLectura">
/// Abre el contenido cuando se necesita. Quien lo abre es responsable de cerrarlo (await using).
/// </param>
public sealed record ArchivoEntrada(
    string Nombre,
    string ContentType,
    long Tamanio,
    Func<Stream> AbrirLectura)
{
    public string Extension => Path.GetExtension(Nombre).TrimStart('.').ToLowerInvariant();

    public string NombreSinExtension => Path.GetFileNameWithoutExtension(Nombre);
}
