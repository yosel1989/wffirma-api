using COFIDE.WFF.Application.Common.Interfaces;
using COFIDE.WFF.Application.Common.Queries;
namespace COFIDE.WFF.Application.Features.Publico.DescargaDocumento;

public sealed record DescargaDocumentoQuery(string token) : BaseQuery<DescargaDocumentoResponseDto>, IAllowAnonymousRequest;