using COFIDE.WFF.Application.Features.Plantillas.Commands.EditarPlantilla;
using COFIDE.WFF.Application.Features.Revisiones.EditarFlujo;
using FirmanteFlujoRequest = COFIDE.WFF.Application.Features.Plantillas.Models.FirmanteFlujoRequest;
using PlantillaEtapaRequest = COFIDE.WFF.Application.Features.Plantillas.Models.PlantillaEtapaRequest;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using COFIDE.WFF.Application.Features.Solicitudes.Mappings;
using COFIDE.WFF.Application.Features.Solicitudes.Models.Flujo;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Inputs;
using FluentAssertions;

namespace COFIDE.WFF.Application.Tests.Common;

public class ApplicationMappingTest
{
    [Fact]
    public void Construir_Solicitud_Debe_Limpiar_Codigos_Y_Correos()
    {
        var input = SolicitudMapping.ConstruirSolicitudInput(
            new SolicitudRequestDto(
                IdTipoSolicitud: 1,
                IdSubTipoSolicitud: 2,
                IdtipoOperacion: 3,
                Asunto: "  Contrato  ",
                CodigosReferencia: [" REF-1 ", "ref-1", " ", "REF-2"],
                Correos: [" a@cofide.com.pe ", "A@cofide.com.pe", ""]),
            new List<DocumentoProcesadoInput>(),
            new List<SustentoProcesadoInput>());

        input.Asunto.Should().Be("Contrato");
        input.CodigosReferencia.Should().Equal("REF-1", "REF-2");
        input.Correos.Should().Equal("a@cofide.com.pe");
    }

    [Fact]
    public void Mapear_Edicion_Sin_Listas_Debe_Quedar_Vacio()
    {
        var modelo = SolicitudMapping.MapearEditarSolicitudModel(
            new SolicitudRequestDto(IdTipoSolicitud: 1, IdSubTipoSolicitud: 2, Asunto: "Contrato"),
            new List<DocumentoProcesadoInput>(),
            new List<SustentoProcesadoInput>());

        modelo.DocumentoIdsConservar.Should().BeEmpty();
        modelo.SustentoIdsConservar.Should().BeEmpty();
        modelo.CodigosReferencia.Should().BeEmpty();
        modelo.Correos.Should().BeEmpty();
        modelo.Asunto.Should().Be("Contrato");
    }

    [Fact]
    public void Mapear_Flujo_Debe_Completar_El_Identificador_Temporal()
    {
        var plantilla = Plantilla.Crear(7, "Flujo", "Descripcion", false, 10, "GTI", false);
        var comando = new EditarFlujoCommand(8, 7, new List<SolicitudEtapaRequest>
        {
            new(3, Guid.Empty, 1, "Etapa 1", 1, 2, new List<SolicitudFirmanteFlujoRequest>
            {
                new(9, 100, "Analista", "GTI", "GERENCIA", 100)
            })
        });

        var input = EditarFlujoMapping.MapearInputPlantilla(comando, plantilla);
        var edicion = EditarFlujoMapping.MapearEditPlantilla(comando);

        input.NombrePlantilla.Should().Be("Flujo");
        input.Etapas.Should().ContainSingle(e => e.NombreEtapa == "Etapa 1" && e.Firmantes.Single().IdUsuarioAsignado == 100);
        edicion.EtapasRequest.Single().TempId.Should().NotBe(Guid.Empty);
        edicion.EtapasRequest.Single().Firmantes.Single().Id.Should().Be(9);
    }

    [Fact]
    public void Mapear_Plantilla_Debe_Conservar_Etapa_Y_Firmante()
    {
        var comando = new EditarPlantillaCommand(6, "Flujo", "Descripcion", new List<PlantillaEtapaRequest>
        {
            new()
            {
                OrdenEtapa = 1,
                NombreEtapa = "Etapa 1",
                IdTipoEtapa = 1,
                IdTipoAprobador = 2,
                Firmantes = new List<FirmanteFlujoRequest>
                {
                    new() { IdUsuarioAsignado = 100, IdUsuarioOrigen = 100 }
                }
            }
        });

        var edicion = EditarPlantillaMapping.MapearEditPlantilla(comando);

        edicion.Id.Should().Be(6);
        edicion.NombrePlantilla.Should().Be("Flujo");
        var etapa = edicion.EtapasRequest.Single();
        etapa.Id.Should().Be(0);
        etapa.Firmantes.Single().Id.Should().Be(0);
        etapa.Firmantes.Single().IdUsuarioAsignado.Should().Be(100);
    }
}
