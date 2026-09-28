using AutoMapper;
using COFIDE.WFF.Application.Common.Handlers;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
namespace COFIDE.WFF.Application.Features.Firmas.Commands.ValidarEstadoDetalleFirma;

public class ValidarEstadoDetalleFirmaCommandHandler(
        IMapper mapper,
        ICurrentUserService _currentUser,
        IDetalleFirmaRepository detalleFirmaRepository
    ) : BaseCommandHandler<ValidarEstadoDetalleFirmaCommand, ValidarEstadoDetalleFirmaDto?>(mapper)
{
    private readonly ICurrentUserService _currentUser = _currentUser;
    private readonly IDetalleFirmaRepository _detalleFirmaRepository = detalleFirmaRepository;

    public override async Task<ValidarEstadoDetalleFirmaDto?> Handle(
     ValidarEstadoDetalleFirmaCommand cmd,
     CancellationToken cancellationToken)
    {
        var detallesEnFirma = await _detalleFirmaRepository
            .ValidarDocumentoFirmadoAsync(cmd.Documentos, cancellationToken);

        var idUsuario = _currentUser.UserIdLocal;

        var detalleEnFirma = detallesEnFirma.LastOrDefault();

        if (detalleEnFirma is null)
            return new ValidarEstadoDetalleFirmaDto
            {
                DocumentoBloqueado = false
            };

        if (idUsuario == detalleEnFirma.IdFirmanteDocumento)
            return new ValidarEstadoDetalleFirmaDto
            {
                DocumentoBloqueado = false
            };

        var nombreCompleto = $"{detalleEnFirma.UsuarioFirmante?.Nombres} {detalleEnFirma.UsuarioFirmante?.ApellidoPaterno}".Trim();

        return new ValidarEstadoDetalleFirmaDto
        {
            DocumentoBloqueado = true,
            NombreFirmante = detalleEnFirma.UsuarioFirmante?.Nombres,
            ApellidosFirmante = detalleEnFirma.UsuarioFirmante?.ApellidoPaterno,
            Mensaje = $"Documento bloqueado. Documento en proceso de firma por {nombreCompleto}. Intente en unos minutos."
        };
    }
}
