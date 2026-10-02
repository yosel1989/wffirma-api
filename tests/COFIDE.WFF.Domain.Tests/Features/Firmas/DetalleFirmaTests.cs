using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Exceptions;
using COFIDE.WFF.Domain.Tests.Common.TestSupport;
using COFIDE.WFF.Domain.ValueObjects;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Features.Firmas;

public sealed record DetalleFirmaCase(
    string Caso,
    bool EsInterno,
    string Nombre,
    string Extension,
    string Ruta,
    string Hash,
    string InternoExternoEsperado)
{
    public override string ToString() => Caso;
}

public class DetalleFirmaTests
{
    public static IEnumerable<object[]> Casos =>
        JsonResource.Cases<DetalleFirmaCase>("Firmas/detalle-firma.json");

    [Theory]
    [MemberData(nameof(Casos))]
    public void Firmar_Debe_Registrar_El_Documento_Y_El_Origen(DetalleFirmaCase caso)
    {
        var detalle = Nuevo();

        detalle.Firmar(caso.Nombre, caso.Extension, caso.Ruta, caso.Hash, caso.EsInterno);

        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Firmado);
        detalle.TieneDocumentoFirmado().Should().BeTrue();
        detalle.NombreDocumentoFirmado.Should().Be(caso.Nombre);
        detalle.HashDocumentoFirmado.Should().Be(caso.Hash);
        detalle.InternoExterno!.ToString().Should().Be(caso.InternoExternoEsperado);
        detalle.FechaHoraFirma.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void Firmar_Dos_Veces_Debe_Lanzar_DOM_SOL_027(DetalleFirmaCase caso)
    {
        var detalle = Nuevo();
        detalle.Firmar(caso.Nombre, caso.Extension, caso.Ruta, caso.Hash, caso.EsInterno);

        var act = () => detalle.Firmar(caso.Nombre, caso.Extension, caso.Ruta, caso.Hash, caso.EsInterno);

        act.Should().Throw<DomainException>()
           .Which.Error.Code.Should().Be("DOM_SOL_027");
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void RestablecerFirma_Debe_Dejar_El_Detalle_Listo_Para_Firmar_De_Nuevo(DetalleFirmaCase caso)
    {
        var detalle = Nuevo();
        detalle.Firmar(caso.Nombre, caso.Extension, caso.Ruta, caso.Hash, caso.EsInterno);
        detalle.SetearCodigoFirma("COD-FIRMA-123");
        detalle.FechaInicioFechaFin(3600);

        detalle.RestablecerFirma();

        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
        detalle.InternoExterno.Should().BeNull();
        detalle.FechaHoraFirma.Should().BeNull();
        detalle.FechaInicioProceso.Should().BeNull();
        detalle.FechaFinEsperado.Should().BeNull();
        detalle.CodigoFirma.Should().BeEmpty();
        detalle.HashDocumentoFirmado.Should().BeEmpty();
        detalle.RutaDocumentoFirmado.Should().BeEmpty();

        // Y debe poder volver a firmarse sin DOM_SOL_027.
        var act = () => detalle.Firmar(caso.Nombre, caso.Extension, caso.Ruta, caso.Hash, caso.EsInterno);
        act.Should().NotThrow();
    }

    [Fact]
    public void Crear_Debe_Generar_Un_CodigoReferencial_Unico_Y_Estado_Pendiente()
    {
        var primero = Nuevo();
        var segundo = Nuevo(id: 2);

        primero.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
        primero.Estado.Should().BeTrue();
        primero.EsNuevo.Should().BeTrue();
        primero.CodigoReferencial.Should().NotBeNullOrWhiteSpace();
        primero.CodigoReferencial.Should().NotBe(segundo.CodigoReferencial);
    }

    [Fact]
    public void FechaInicioFechaFin_Debe_Proyectar_El_Vencimiento_Con_El_Tiempo_Recibido()
    {
        var detalle = Nuevo();

        detalle.FechaInicioFechaFin(tiempoFirma: 600);

        detalle.FechaFinEsperado.Should()
            .BeCloseTo(detalle.FechaInicioProceso!.Value.AddSeconds(600), TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(10, 0)]
    public void Crear_Con_Identificadores_No_Positivos_Debe_Lanzar_ArgumentException(
        long idFirmanteFlujo, long idDocumento)
    {
        // Nota: aquí el dominio usa ArgumentException, no DomainException.
        var act = () => DetalleFirma.Crear(1, idFirmanteFlujo, idDocumento, null, null, null, null, 100);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Bloquear_Y_ActivarDocumento_Deben_Alternar_El_Estado_Del_Documento()
    {
        var detalle = Nuevo();

        detalle.Bloquear();
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Bloqueado);

        detalle.ActivarDocumento();
        detalle.EstadoDocumento.Should().Be(EstadoDocumento.Pendiente);
    }

    private static DetalleFirma Nuevo(long id = 1) =>
        DetalleFirma.Crear(id, idSolicitudFirmanteFlujo: 10, idSolicitudDocumento: 20,
            null, null, null, null, idFirmanteDocumento: 100);
}
