using COFIDE.WFF.Infrastructure.Exceptions;
using COFIDE.WFF.Infrastructure.Tests.Support;
using FluentAssertions;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.FileStorage;

/// <summary>Operaciones básicas del adaptador de archivos sobre una carpeta temporal real.</summary>
public sealed class FileStorageServiceTests : IDisposable
{
    private readonly DirectorioTemporal _directorio = new();

    [Fact]
    public async Task Guardar_Debe_Crear_La_Carpeta_Y_Devolver_La_Ruta_Completa()
    {
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        var ruta = await sut.SaveFileAsync(AlmacenamientoDePrueba.Contenido("hola"), "a.pdf", "SOL-1", "a.pdf");

        ruta.Should().Be(_directorio.Combinar("SOL-1", "a.pdf"));
        (await File.ReadAllTextAsync(ruta)).Should().Be("hola");
    }

    [Fact]
    public async Task Guardar_Sin_Nombre_Debe_Fallar()
    {
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        var act = () => sut.SaveFileAsync(AlmacenamientoDePrueba.Contenido("hola"), "a.pdf", "SOL-1", "");

        await act.Should().ThrowAsync<InfrastructureException>();
    }

    [Fact]
    public async Task Leer_Debe_Usar_Rutas_Relativas_A_La_Base()
    {
        _directorio.CrearArchivo(Path.Combine("SOL-1", "a.pdf"), "contenido");
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        await using var stream = await sut.GetFileAsync(Path.Combine("SOL-1", "a.pdf"));
        using var lector = new StreamReader(stream);

        (await lector.ReadToEndAsync()).Should().Be("contenido");
    }

    [Fact]
    public async Task Leer_Un_Archivo_Inexistente_Debe_Fallar()
    {
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        var act = () => sut.GetFileAsync(Path.Combine("SOL-1", "no-existe.pdf"));

        await act.Should().ThrowAsync<InfrastructureException>();
    }

    [Fact]
    public async Task Eliminar_Debe_Indicar_Si_El_Archivo_Existia()
    {
        _directorio.CrearArchivo(Path.Combine("SOL-1", "a.pdf"), "contenido");
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        (await sut.DeleteFileAsync(Path.Combine("SOL-1", "a.pdf"))).Should().BeTrue();
        (await sut.DeleteFileAsync(Path.Combine("SOL-1", "a.pdf"))).Should().BeFalse();
    }

    [Theory]
    [InlineData("SOL-1", "a", "pdf", true)]
    [InlineData("SOL-1", "b", "pdf", false)]
    [InlineData("", "a", "pdf", false)]
    [InlineData("SOL-1", "", "pdf", false)]
    [InlineData("SOL-1", "a", "", false)]
    public async Task Existe_Debe_Validar_Ruta_Nombre_Y_Extension(string ruta, string nombre, string extension, bool esperado)
    {
        _directorio.CrearArchivo(Path.Combine("SOL-1", "a.pdf"), "contenido");
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        (await sut.ExistsFileAsync(ruta, nombre, extension)).Should().Be(esperado);
    }

    [Fact]
    public void Estructura_De_Carpetas_Debe_Quedar_Dentro_Del_Adaptador()
    {
        var sut = AlmacenamientoDePrueba.Crear(_directorio);

        sut.ObtenerCarpetaDocumentos("SOL-1").Should().Be(Path.Combine("SOL-1", "DOCUMENTOS"));
        sut.ObtenerCarpetaSustentos("SOL-1").Should().Be(Path.Combine("SOL-1", "SUSTENTOS"));
        sut.CombinarRuta("SOL-1", "a.pdf").Should().Be(Path.Combine("SOL-1", "a.pdf"));
    }

    public void Dispose() => _directorio.Dispose();
}
