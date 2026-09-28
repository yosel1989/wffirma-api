namespace COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;

public interface IPdfContentHashService
{
    string ObtenerTexto(byte[] pdfBytes);
    string ObtenerTexto(Stream pdfStream);
    string ObtenerHashContenido(byte[] pdfBytes);    
    string ObtenerHashContenido(Stream pdfStream);
}
