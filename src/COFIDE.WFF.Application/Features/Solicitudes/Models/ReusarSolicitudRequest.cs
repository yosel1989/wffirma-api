using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;
using Microsoft.AspNetCore.Http;

namespace COFIDE.WFF.Application.Features.Solicitudes.Models;

public class ReusarSolicitudRequest
{
    public bool InCodigo { get; set; }

    public List<IFormFile>? Documentos { get; set; }
    public List<IFormFile>? Sustentos { get; set; }

    public List<long>? IdsDocumentosConservar { get; set; }
    public List<long>? IdsSustentosConservar { get; set; }

    public List<string>? CodigosReferencia { get; set; }
    public List<string>? Correos { get; set; }
}