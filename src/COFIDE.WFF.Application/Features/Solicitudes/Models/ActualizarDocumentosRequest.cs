using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Models;
public class ActualizarDocumentosRequest
{
    public List<long>? IdsDocumentosConservar { get; set; }
    public List<IFormFile>? Documentos { get; set; }
    public bool InCodigo { get; set; }
}