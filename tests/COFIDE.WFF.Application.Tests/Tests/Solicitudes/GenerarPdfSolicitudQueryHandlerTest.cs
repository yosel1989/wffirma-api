using AutoMapper;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Exceptions;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Ports.Out;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerSubTipoSolicitudPorArea;
using COFIDE.WFF.Application.Features.Maestros.Queries.ObtenerTipoOperacionPorArea;
using COFIDE.WFF.Application.Features.Solicitudes.Models.Pdf;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.GenerarPdfSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerSolicitud;
using COFIDE.WFF.Application.Tests.Support;
using COFIDE.WFF.Domain.Entities;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class GenerarPdfSolicitudQueryHandlerTest
{
    private readonly Mock<ICurrentUserService> _usuario = new();
    private readonly Mock<ISolicitudQueryRepository> _consultas = new();
    private readonly Mock<IMaestroRepository> _maestros = new();
    private readonly Mock<ISolicitudPdfGenerator> _generador = new();
    private readonly Mock<IAccesoEspecialRepository> _accesoEspecialRepo = new();
    private SolicitudPdf? _modelo;

    public GenerarPdfSolicitudQueryHandlerTest()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns(10);
        _generador.Setup(g => g.Generar(It.IsAny<SolicitudPdf>()))
            .Callback<SolicitudPdf>(m => _modelo = m)
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        var tipo = (TipoSolicitud)Activator.CreateInstance(typeof(TipoSolicitud), nonPublic: true)!;
        NavegacionEf.Asignar(tipo, t => t.NombreTipoSolicitud, "Órdenes de compra");
        _maestros.Setup(m => m.GetTipoSolicitudByIdAsync(1, It.IsAny<CancellationToken>(), false)).ReturnsAsync(tipo);
        _maestros.Setup(m => m.GetSubTipoSolicitudCombo(null, It.IsAny<List<long>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubTipoSolicitudComboDto> { new() { Id = 2, Nombre = "Bienes" } });
        _maestros.Setup(m => m.GetTipoOperacionCombo(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoOperacionComboDto> { new() { Id = 3, Nombre = "Nuevo" } });
    }

    [Fact]
    public async Task Debe_Armar_El_Modelo_Con_Los_Datos_De_La_Solicitud()
    {
        _consultas.Setup(c => c.ObtenerSolicitudPorId(8, 10, false, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(Solicitud());

        var resultado = await Handler().Handle(new GenerarPdfSolicitudQuery(8), CancellationToken.None);

        resultado!.NombreArchivo.Should().Be("Solicitud_SOL-1.pdf");
        resultado.ContentType.Should().Be("application/pdf");
        _modelo!.EsUrgente.Should().BeTrue();
        _modelo.TipoSolicitud.Should().Be("Órdenes de compra");
        _modelo.SubTipoSolicitud.Should().Be("Bienes");
        _modelo.TipoOperacion.Should().Be("Nuevo");
        _modelo.ArchivosFirma.Should().ContainSingle().Which.Nombre.Should().Be("contrato.pdf");
        _modelo.ArchivosSustento.Should().ContainSingle().Which.Nombre.Should().Be("anexo.xlsx");
        _modelo.CodigosReferencia.Should().Equal("2025-2000-1900037134");
        _modelo.Destinatarios.Should().Equal("carlos@cofide.com.pe");
    }

    [Fact]
    public async Task Debe_Traducir_El_Estado_De_Cada_Aprobador()
    {
        _consultas.Setup(c => c.ObtenerSolicitudPorId(8, 10, false, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(Solicitud());

        await Handler().Handle(new GenerarPdfSolicitudQuery(8), CancellationToken.None);

        var aprobadores = _modelo!.Etapas.Single().Aprobadores;
        aprobadores.Select(a => a.Estado).Should().Equal(
            EstadoAprobadorPdf.Aprobado, EstadoAprobadorPdf.TuTurno, EstadoAprobadorPdf.Pendiente);
        aprobadores[0].Detalle.Should().Be("19/11/2025 16:35");
        aprobadores[0].Nombre.Should().Be("Mariana Salazar Ruiz");
    }

    [Fact]
    public async Task Rol_Del_Firmante_Debe_Ser_Su_Cargo_O_El_Tipo_De_Aprobador()
    {
        _consultas.Setup(c => c.ObtenerSolicitudPorId(8, 10, false, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(Solicitud());

        await Handler().Handle(new GenerarPdfSolicitudQuery(8), CancellationToken.None);

        _modelo!.Firmantes.Select(f => f.Rol).Should().Equal("Administrador", "Firmante", "Firmante");
        _modelo.Firmantes.Should().OnlyContain(f => f.Orden == 1);
    }

    [Fact]
    public async Task Solicitud_Inexistente_Debe_Devolver_Null_Sin_Generar_El_Pdf()
    {
        var resultado = await Handler().Handle(new GenerarPdfSolicitudQuery(99), CancellationToken.None);

        resultado.Should().BeNull();
        _generador.Verify(g => g.Generar(It.IsAny<SolicitudPdf>()), Times.Never);
    }

    [Fact]
    public async Task Sin_Usuario_Autenticado_Debe_Fallar()
    {
        _usuario.Setup(u => u.UserIdLocal).Returns((long?)null);

        var act = () => Handler().Handle(new GenerarPdfSolicitudQuery(8), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationExceptionBase>();
    }

    private GenerarPdfSolicitudQueryHandler Handler() =>
        new(new Mock<IMapper>().Object, _usuario.Object, _consultas.Object, _maestros.Object, _generador.Object, _accesoEspecialRepo.Object);

    private static SolicitudDto Solicitud() => new()
    {
        IdSolicitud = 8,
        IdTipoSolicitud = 1,
        IdSubTipoSolicitud = 2,
        IdtipoOperacion = 3,
        CodigoSolicitud = "SOL-1",
        Asunto = "Autorización de pago a proveedores de RRHH",
        Mensaje = "<p>Se ha registrado el orden de personas a firmar.</p>",
        EsUrgente = true,
        FechaLimite = new DateTime(2026, 10, 24),
        Documentos = [new ArchivoDto { Id = 1, Nombre = "contrato", Extension = "pdf", Peso = 229_376 }],
        Sustentos = [new SustentoDto { Id = 2, Nombre = "anexo", Extension = "xlsx", Peso = 10_240 }],
        CodigosReferencia = [new CodigosReferenciaDto { Id = 1, Nombre = "2025-2000-1900037134" }],
        SolicitudCorreo = [new CorreoDto { Id = 1, Nombre = "carlos@cofide.com.pe" }],
        EstadoFlujo = new EstadoFlujov2Dto
        {
            etapas =
            [
                new PlantillaEtapav2Dto
                {
                    OrdenEtapa = 1,
                    ValorTipoEtapa = "Secuencial",
                    ValorTipoAprobador = "Firmante",
                    Firmantes =
                    [
                        new FirmanteFlujov2Dto { Nombres = "Mariana", ApellidoPaterno = "Salazar", ApellidoMaterno = "Ruiz", CargoUsuario = "Administrador", Estado = "19/11/2025 16:35" },
                        new FirmanteFlujov2Dto { Nombres = "Samuel", ApellidoPaterno = "Gutiérrez", Estado = "Tu Turno" },
                        new FirmanteFlujov2Dto { Nombres = "Ingrid", ApellidoPaterno = "Beltrán", Estado = "Pendiente" }
                    ]
                }
            ]
        }
    };
}
