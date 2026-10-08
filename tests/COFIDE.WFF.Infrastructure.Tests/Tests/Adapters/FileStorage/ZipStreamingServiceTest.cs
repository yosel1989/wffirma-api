using System.IO.Compression;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Infrastructure.Exceptions;
using COFIDE.WFF.Infrastructure.Tests.Support;
using FluentAssertions;

using COFIDE.WFF.Application.Common.Files;
namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.FileStorage;

/// <summary>H2: el ZIP lo arma Infraestructura con los nombres y carpetas que decide Application.</summary>
public sealed class ZipStreamingServiceTest : IDisposable
{
    private readonly DirectorioTemporal _directorio = new();

    [Fact]
    public async Task GenerarZip_Debe_Incluir_Cada_Archivo_Con_El_Nombre_Indicado()
    {
        _directorio.CrearArchivo(Path.Combine("SOL-1", "DOCUMENTOS", "contrato.pdf"), "doc");
        _directorio.CrearArchivo(Path.Combine("SOL-1", "SUSTENTOS", "anexo.pdf"), "sus");
        var sut = AlmacenamientoDePrueba.CrearZip(AlmacenamientoDePrueba.Crear(_directorio));

        var bytes = await sut.GenerarZipAsync(
        [
            new EntradaZip("DOCUMENTOS/contrato.pdf", Path.Combine("SOL-1", "DOCUMENTOS", "contrato.pdf")),
            new EntradaZip("SUSTENTOS/anexo.pdf", Path.Combine("SOL-1", "SUSTENTOS", "anexo.pdf"))
        ], CancellationToken.None);

        var contenido = Leer(bytes);
        contenido.Should().HaveCount(2);
        contenido["DOCUMENTOS/contrato.pdf"].Should().Be("doc");
        contenido["SUSTENTOS/anexo.pdf"].Should().Be("sus");
    }

    [Fact]
    public async Task GenerarZip_Sin_Entradas_Debe_Devolver_Un_Zip_Vacio_Valido()
    {
        var sut = AlmacenamientoDePrueba.CrearZip(AlmacenamientoDePrueba.Crear(_directorio));

        var bytes = await sut.GenerarZipAsync([], CancellationToken.None);

        Leer(bytes).Should().BeEmpty();
    }

    [Fact]
    public async Task GenerarZip_Con_Un_Archivo_Faltante_Debe_Fallar()
    {
        var sut = AlmacenamientoDePrueba.CrearZip(AlmacenamientoDePrueba.Crear(_directorio));

        var act = () => sut.GenerarZipAsync(
            [new EntradaZip("DOCUMENTOS/x.pdf", Path.Combine("SOL-1", "x.pdf"))],
            CancellationToken.None);

        await act.Should().ThrowAsync<InfrastructureException>();
    }

    private static Dictionary<string, string> Leer(byte[] zip)
    {
        using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        return archive.Entries.ToDictionary(
            e => e.FullName,
            e => new StreamReader(e.Open()).ReadToEnd());
    }

    public void Dispose() => _directorio.Dispose();
}
