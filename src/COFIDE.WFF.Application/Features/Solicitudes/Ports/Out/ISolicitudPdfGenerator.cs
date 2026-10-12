using COFIDE.WFF.Application.Features.Solicitudes.Models.Pdf;
namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

/// <summary>Genera el formato de la solicitud en PDF.</summary>
public interface ISolicitudPdfGenerator
{
    byte[] Generar(SolicitudPdf solicitud);
}
