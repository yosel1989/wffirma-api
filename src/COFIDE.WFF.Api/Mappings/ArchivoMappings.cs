using COFIDE.WFF.Application.Common.Files;

namespace COFIDE.WFF.Api.Mappings
{
    public static class ArchivoMappings
    {
        public static ArchivoEntrada ToArchivoEntrada(this IFormFile archivo) =>
            new(
                Path.GetFileName(archivo.FileName),
                archivo.ContentType,
                archivo.Length,
                archivo.OpenReadStream);

        /// <summary>Conserva null cuando no se enviaron archivos (mismo comportamiento que antes).</summary>
        public static List<ArchivoEntrada>? ToArchivosEntrada(this IEnumerable<IFormFile>? archivos) =>
            archivos?.Select(ToArchivoEntrada).ToList();
    }

}
