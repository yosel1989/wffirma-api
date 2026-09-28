using COFIDE.WFF.Application.Features.Firmas.DTOs;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
namespace COFIDE.WFF.Application.Features.Firmas.Services;

public interface IFirmaPreparationService
{
    Task<FirmarDocumentoRequestDto> PrepararDocumentoFirmaAsync(
        SolicitudDocumento solicitudDocumento,
        string codigoReferencial,
        long idUsuario,
        int cantidadDocumentos,
        InformacionPaginaPdfDto? informacionPagina,
        CancellationToken cancellationToken);
}