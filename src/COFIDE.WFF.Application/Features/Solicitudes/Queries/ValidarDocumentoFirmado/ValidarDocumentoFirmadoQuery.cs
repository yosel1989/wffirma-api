using COFIDE.WFF.Application.Common.Queries;
using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;
public sealed record ValidarDocumentoFirmadoQuery(List<IFormFile> Archivos) : BaseQuery<IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>>;
