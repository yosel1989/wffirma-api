using COFIDE.WFF.Application.Common.Queries;
using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;
public sealed record ValidarDocumentoFirmadoQuery(List<ArchivoEntrada> Archivos) : BaseQuery<IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>>;
