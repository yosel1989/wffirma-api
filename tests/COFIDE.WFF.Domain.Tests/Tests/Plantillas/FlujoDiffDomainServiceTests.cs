using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Models;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Plantillas;

/// <summary>
/// A2: el cálculo de diferencias del flujo ahora es un servicio de dominio.
/// Plantilla base: etapa 1 (orden 1, usuario 100) y etapa 2 (orden 2, usuario 101).
/// </summary>
public class FlujoDiffDomainServiceTests
{
    private readonly FlujoDiffDomainService _sut = new();

    [Fact]
    public void Sin_Cambios_No_Debe_Reportar_Diferencias()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1, 2);

        var diff = _sut.CalcularDiff(plantilla, Request(Etapa(1, 1, 100), Etapa(2, 2, 101)));

        diff.EtapasAgregadas.Should().BeEmpty();
        diff.EtapasEliminadas.Should().BeEmpty();
        diff.FirmantesAgregados.Should().BeEmpty();
        diff.FirmantesEliminados.Should().BeEmpty();
        diff.EtapasMovidas.Should().BeEmpty();
    }

    [Fact]
    public void Etapa_Nueva_Debe_Reportar_La_Etapa_Y_Sus_Firmantes()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1, 2);
        var nueva = Etapa(0, 3, 200);

        var diff = _sut.CalcularDiff(plantilla, Request(Etapa(1, 1, 100), Etapa(2, 2, 101), nueva));

        diff.EtapasAgregadas.Should().ContainSingle()
            .Which.Should().Match<EtapaAgregada>(e => e.Orden == 3 && e.TempId == nueva.TempId);
        diff.FirmantesAgregados.Should().ContainSingle()
            .Which.Should().Match<FirmanteAgregado>(f => f.IdUsuario == 200 && f.TempIdEtapa == nueva.TempId);
    }

    [Fact]
    public void Etapa_Quitada_Debe_Reportar_La_Etapa_Y_Sus_Firmantes_Como_Eliminados()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1, 2);

        var diff = _sut.CalcularDiff(plantilla, Request(Etapa(1, 1, 100)));

        diff.EtapasEliminadas.Should().ContainSingle().Which.IdEtapa.Should().Be(2);
        diff.FirmantesEliminados.Should().ContainSingle()
            .Which.Should().Match<FirmanteEliminado>(f => f.IdUsuario == 101 && f.IdEtapa == 2);
    }

    [Fact]
    public void Cambio_De_Firmante_En_Una_Etapa_Debe_Reportar_Salida_Y_Entrada()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1, 2);

        var diff = _sut.CalcularDiff(plantilla, Request(Etapa(1, 1, 150), Etapa(2, 2, 101)));

        diff.FirmantesEliminados.Should().ContainSingle()
            .Which.Should().Match<FirmanteEliminado>(f => f.IdUsuario == 100 && f.IdEtapa == 1);
        diff.FirmantesAgregados.Should().ContainSingle()
            .Which.Should().Match<FirmanteAgregado>(f => f.IdUsuario == 150 && f.IdEtapa == 1);
        diff.EtapasAgregadas.Should().BeEmpty();
        diff.EtapasEliminadas.Should().BeEmpty();
    }

    [Fact]
    public void Etapas_Reordenadas_Deben_Reportarse_Como_Movidas()
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(1, 2);

        var diff = _sut.CalcularDiff(plantilla, Request(Etapa(1, 2, 100), Etapa(2, 1, 101)));

        diff.EtapasMovidas.Should().HaveCount(2);
        diff.EtapasMovidas.Should().Contain(m => m.IdEtapa == 1 && m.OrdenAnterior == 1 && m.OrdenNuevo == 2);
        diff.EtapasMovidas.Should().Contain(m => m.IdEtapa == 2 && m.OrdenAnterior == 2 && m.OrdenNuevo == 1);
    }

    private static PlantillaEditModel Request(params EtapaEditModel[] etapas) =>
        new() { Id = 1, NombrePlantilla = "Flujo", EtapasRequest = etapas.ToList() };

    private static EtapaEditModel Etapa(long id, long orden, params long[] usuarios) =>
        new()
        {
            Id = id,
            TempId = Guid.NewGuid(),
            Orden = orden,
            Nombre = $"Etapa {orden}",
            IdTipoEtapa = 1,
            IdTipoAprobador = 10,
            Firmantes = usuarios.Select(u => new FirmanteEditModel { IdUsuarioAsignado = u, IdUsuarioOrigen = 10 }).ToList()
        };
}
