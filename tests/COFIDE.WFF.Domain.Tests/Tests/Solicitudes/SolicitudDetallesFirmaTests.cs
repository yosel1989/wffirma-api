using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Plantillas;
using COFIDE.WFF.Domain.Tests.Support.Mothers.Solicitudes;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Solicitudes;

/// <summary>A2: la generación de detalles de firma pasó de SolicitudPlantillaService al agregado.</summary>
public class SolicitudDetallesFirmaTests
{
    [Fact]
    public void Debe_Crear_Un_Detalle_Por_Firmante_Y_Documento()
    {
        var solicitud = ConFlujoYDocumentos(etapas: [1, 2], documentos: 2);

        solicitud.GenerarDetallesFirmaPendientes(SolicitudMother.Ids(900));

        var firmantes = solicitud.SolicitudPlantilla.FirmantesFlujo.Select(f => f.Id).ToList();
        foreach (var documento in solicitud.Documentos)
        {
            documento.DetallesFirma.Should().HaveCount(2);
            documento.DetallesFirma.Select(d => d.IdSolicitudFirmanteFlujo).Should().BeEquivalentTo(firmantes);
        }
    }

    [Fact]
    public void Llamarlo_Dos_Veces_No_Debe_Duplicar_Detalles()
    {
        var solicitud = ConFlujoYDocumentos(etapas: [1, 2], documentos: 1);
        var ids = SolicitudMother.Ids(900);

        solicitud.GenerarDetallesFirmaPendientes(ids);
        solicitud.GenerarDetallesFirmaPendientes(ids);

        solicitud.Documentos.Single().DetallesFirma.Should().HaveCount(2);
    }

    [Fact]
    public void Ids_De_Los_Detalles_Deben_Ser_Unicos()
    {
        var solicitud = ConFlujoYDocumentos(etapas: [1, 2, 3], documentos: 2);

        solicitud.GenerarDetallesFirmaPendientes(SolicitudMother.Ids(900));

        solicitud.Documentos.SelectMany(d => d.DetallesFirma).Select(d => d.Id)
            .Should().HaveCount(6).And.OnlyHaveUniqueItems();
    }

    private static Solicitud ConFlujoYDocumentos(long[] etapas, int documentos)
    {
        var plantilla = PlantillaMother.ConEtapasSecuenciales(etapas);
        var solicitud = SolicitudMother.Borrador();
        solicitud.AsignarPlantilla(SolicitudPlantilla.Crear(1, solicitud, plantilla));
        new WorkflowDomainService().AgregarFirmantesFlujoDesdeEtapas(solicitud, plantilla, SolicitudMother.Ids(500));

        for (var i = 1; i <= documentos; i++)
            solicitud.AgregarDocumento(id: i, nombre: $"documento{i}");

        return solicitud;
    }
}
