using COFIDE.WFF.Application.Features.Firmas.DTOs;

namespace COFIDE.WFF.Application.Features.Firmas.Ports.Out;

public interface IOrientacionPaginaPdfService
{
    InformacionPaginaPdfDto ObtenerPrimeraPagina(Stream pdfStream);
}