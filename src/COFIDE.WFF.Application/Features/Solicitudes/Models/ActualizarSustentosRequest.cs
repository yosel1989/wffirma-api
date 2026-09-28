using Microsoft.AspNetCore.Http;
namespace COFIDE.WFF.Application.Features.Solicitudes.Models;

public class ActualizarSustentosRequest
{
    public List<long>? IdsSustentosConservar { get; set; }
    public List<IFormFile>? Sustentos { get; set; }
}