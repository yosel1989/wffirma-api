using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

/// <summary>
/// Regla de dependencias a nivel de código (IL): ningún tipo del núcleo usa
/// tipos de las capas externas ni de librerías de tecnología de adaptadores.
/// </summary>
[Trait("Categoria", "Hexagonal")]
public class DependenciasEntreCapasTests
{
    private static readonly string[] CapasExternasAlDominio =
        { Capas.Application, Capas.Infrastructure, Capas.Api, Capas.Worker };

    private static readonly string[] CapasExternasAApplication =
        { Capas.Infrastructure, Capas.Api, Capas.Worker };

    private static readonly string[] TecnologiaProhibidaEnDominio =
    {
        "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "Microsoft.Extensions", "System.Net.Http",
        "Oracle", "MediatR", "AutoMapper", "FluentValidation", "Newtonsoft.Json", "System.Text.Json",
        "Serilog", "System.IO.Compression", "System.Data",
    };

    private static readonly string[] TecnologiaProhibidaEnApplication =
    {
        "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Oracle", "System.Net.Http",
        "Serilog", "PdfSharp", "Org.BouncyCastle", "System.Data", "System.ServiceModel",
    };

    [Fact, Trait("Hallazgo", "Base")]
    public void Domain_no_depende_de_otras_capas()
    {
        ArchitectureAssert.SinViolaciones(
            "Base - Domain no depende de otras capas",
            Dependencias.Prohibidas(Capas.DomainAssembly, CapasExternasAlDominio),
            "El dominio no conoce a Application ni a los adaptadores.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Domain_no_depende_de_frameworks_ni_librerias_tecnicas()
    {
        ArchitectureAssert.SinViolaciones(
            "Base - Domain sin tecnología",
            Dependencias.Prohibidas(Capas.DomainAssembly, TecnologiaProhibidaEnDominio),
            "El dominio no usa EF, ASP.NET, HTTP, serialización, MediatR ni logging.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Application_no_depende_de_adaptadores()
    {
        ArchitectureAssert.SinViolaciones(
            "Base - Application no depende de adaptadores",
            Dependencias.Prohibidas(Capas.ApplicationAssembly, CapasExternasAApplication),
            "Application solo conoce Domain y define puertos que implementan los adaptadores.");
    }

    [Fact, Trait("Hallazgo", "H1")]
    public void Application_no_depende_de_tecnologia_de_adaptadores()
    {
        ArchitectureAssert.SinViolaciones(
            "H1 - Application sin tecnología de adaptadores",
            Dependencias.Prohibidas(Capas.ApplicationAssembly, TecnologiaProhibidaEnApplication),
            "Reemplace IFormFile por ArchivoEntrada y mueva el acceso HTTP/datos/PDF detrás de puertos.");
    }

    [Fact, Trait("Hallazgo", "H2")]
    public void Application_no_genera_zip_ni_manipula_compresion()
    {
        ArchitectureAssert.SinViolaciones(
            "H2 - Application sin System.IO.Compression",
            Dependencias.Prohibidas(Capas.ApplicationAssembly, new[] { "System.IO.Compression" }),
            "Delegue la generación de ZIP en IZipStreamingService (Infraestructura).");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Infrastructure_no_depende_de_los_hosts()
    {
        ArchitectureAssert.SinViolaciones(
            "Base - Infrastructure no depende de Api/Worker",
            Dependencias.Prohibidas(Capas.InfrastructureAssembly, new[] { Capas.Api, Capas.Worker }),
            "Los adaptadores no conocen a los hosts.");
    }

    [Fact, Trait("Hallazgo", "Base")]
    public void Controladores_no_usan_Infrastructure()
    {
        ArchitectureAssert.SinViolaciones(
            "Base - Controladores sin Infrastructure",
            Dependencias.Prohibidas(Capas.ApiAssembly, new[] { Capas.Infrastructure }, $"{Capas.Api}.Controllers"),
            "Los controladores solo envían Commands/Queries; la composición con Infrastructure ocurre en Program/DI.");
    }

    [Fact, Trait("Categoria", "Transversal"), Trait("Hallazgo", "C1")]
    public void Middleware_no_depende_de_excepciones_de_Infrastructure()
    {
        ArchitectureAssert.SinViolaciones(
            "C1 - Middleware sin excepciones de Infrastructure",
            Dependencias.Prohibidas(Capas.ApiAssembly, new[] { $"{Capas.Infrastructure}.Exceptions" }, $"{Capas.Api}.Middleware"),
            "Unifique el modelo de errores en SharedKernel (Error + AppException) y capture una sola excepción base.");
    }
}
