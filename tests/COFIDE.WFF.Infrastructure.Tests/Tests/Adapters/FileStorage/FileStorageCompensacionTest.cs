using COFIDE.WFF.Infrastructure.Adapters.Services.FileStorage;
using COFIDE.WFF.Infrastructure.Tests.Support;
using FluentAssertions;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.FileStorage;

/// <summary>B1: si la transacción se revierte, el almacenamiento debe quedar como estaba.</summary>
public sealed class FileStorageCompensacionTest : IDisposable
{
    private readonly DirectorioTemporal _directorio = new();
    private readonly FileStorageService _sut;

    public FileStorageCompensacionTest() => _sut = AlmacenamientoDePrueba.Crear(_directorio);

    [Fact]
    public async Task Rollback_Debe_Eliminar_Los_Archivos_Nuevos()
    {
        var ruta = await Guardar("nuevo.pdf", "contenido");

        await _sut.DescartarArchivosAsync(CancellationToken.None);

        File.Exists(ruta).Should().BeFalse();
    }

    [Fact]
    public async Task Rollback_Debe_Restaurar_Los_Archivos_Sobrescritos()
    {
        var ruta = _directorio.CrearArchivo(Path.Combine("SOL-1", "existente.pdf"), "original");

        await Guardar("existente.pdf", "modificado");
        await _sut.DescartarArchivosAsync(CancellationToken.None);

        (await File.ReadAllTextAsync(ruta)).Should().Be("original");
        _directorio.Buscar("*.bak-*").Should().BeEmpty();
    }

    [Fact]
    public async Task Sobrescribir_Dos_Veces_Debe_Restaurar_El_Contenido_Anterior_A_La_Transaccion()
    {
        var ruta = _directorio.CrearArchivo(Path.Combine("SOL-1", "existente.pdf"), "original");

        await Guardar("existente.pdf", "primer cambio");
        await Guardar("existente.pdf", "segundo cambio");
        await _sut.DescartarArchivosAsync(CancellationToken.None);

        (await File.ReadAllTextAsync(ruta)).Should().Be("original");
    }

    [Fact]
    public async Task Commit_Debe_Conservar_Los_Cambios_Y_Borrar_Los_Respaldos()
    {
        var ruta = _directorio.CrearArchivo(Path.Combine("SOL-1", "existente.pdf"), "original");
        var nuevo = await Guardar("nuevo.pdf", "contenido");

        await Guardar("existente.pdf", "modificado");
        _sut.ConfirmarArchivos();

        (await File.ReadAllTextAsync(ruta)).Should().Be("modificado");
        File.Exists(nuevo).Should().BeTrue();
        _directorio.Buscar("*.bak-*").Should().BeEmpty();
    }

    [Fact]
    public async Task Rollback_Despues_De_Confirmar_No_Debe_Tocar_Lo_Confirmado()
    {
        var ruta = await Guardar("nuevo.pdf", "contenido");
        _sut.ConfirmarArchivos();

        await _sut.DescartarArchivosAsync(CancellationToken.None);

        File.Exists(ruta).Should().BeTrue();
    }

    private async Task<string> Guardar(string nombre, string contenido)
    {
        using var stream = AlmacenamientoDePrueba.Contenido(contenido);
        return await _sut.SaveFileAsync(stream, nombre, "SOL-1", nombre);
    }

    public void Dispose() => _directorio.Dispose();
}
