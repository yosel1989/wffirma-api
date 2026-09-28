using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Firmas.Ports.Out;
namespace COFIDE.WFF.Application.Features.Solicitudes.Queries.ValidarDocumentoFirmado;

public sealed class ValidarDocumentoFirmadoQueryHandler(
        IMapper mapper,
        IDocumentoFirmadoValidator documentoFirmadoValidator
    ) : BaseQueryHandler<ValidarDocumentoFirmadoQuery, IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>>(mapper)
{
    private readonly IDocumentoFirmadoValidator _documentoFirmadoValidator = documentoFirmadoValidator;

    public override async Task<IReadOnlyCollection<ValidarDocumentoFirmadoResponseDto>> Handle(
       ValidarDocumentoFirmadoQuery request,
       CancellationToken cancellationToken)
    {
        var resultado = new List<ValidarDocumentoFirmadoResponseDto>();

        foreach (var archivo in request.Archivos)
        {
            await using var stream = archivo.OpenReadStream();

            resultado.Add(new ValidarDocumentoFirmadoResponseDto
            {
                NombreDocumento = archivo.FileName,
                TieneFirmaDigital = await _documentoFirmadoValidator.TieneFirmaAsync(stream)
            });
        }

        return resultado;
    }
}
