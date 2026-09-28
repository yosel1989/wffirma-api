using COFIDE.WFF.Application.Common.Enums;
namespace COFIDE.WFF.Application.Features.Publico.DescargaDocumento;

public sealed record DescargaDocumentoResponseDto(
 TipoDescarga Tipo,
 string NombreArchivo,
 string ContentType,
 IReadOnlyCollection<ArchivoDescargaDto> Archivos);
