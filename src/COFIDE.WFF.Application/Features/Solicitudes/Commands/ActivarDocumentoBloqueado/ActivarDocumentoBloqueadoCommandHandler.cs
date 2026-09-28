using AutoMapper;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using ValidacionAplicacion = COFIDE.WFF.Application.Exceptions;
namespace COFIDE.WFF.Application.Features.Solicitudes.Commands.ActivarDocumentoBloqueado;

public sealed class ActivarDocumentoBloqueadoCommandHandler(
        IMapper mapper,
        IDetalleFirmaRepository detalleFirmaRepository
    ) : BaseCommandHandler<ActivarDocumentoBloqueadoCommand, string>(mapper)
{
    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;

    public override async Task<string> Handle(ActivarDocumentoBloqueadoCommand request, CancellationToken cancellationToken)
    {
        var detalle = await _detalleFirmaRepository.ObtenerDetalleBloqueadoPorDocumentoAsync(request.IdDocumento, cancellationToken);

        if (detalle is null)
            throw new ValidacionAplicacion.ApplicationException(ApplicationErrors.DocumentoBloqueadoNotFound);

        detalle.ActivarDocumento();
        return detalle.SolicitudDocumento.NombreOriginal;
    }
}