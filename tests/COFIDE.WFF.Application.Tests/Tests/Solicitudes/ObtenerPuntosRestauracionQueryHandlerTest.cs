using AutoMapper;
using COFIDE.WFF.Application.Features.Solicitudes.Ports.Out;
using COFIDE.WFF.Application.Features.Solicitudes.Queries.ObtenerPuntosRestauracion;
using FluentAssertions;
using Moq;

namespace COFIDE.WFF.Application.Tests.Solicitudes;

public class ObtenerPuntosRestauracionQueryHandlerTest
{
    [Fact]
    public async Task Handle_Debe_Devolver_Los_Puntos_Del_Repositorio()
    {
        var puntos = new List<PuntoRestauracionDto>
        {
            new()
            {
                IdPuntoRestauracion = 1718,
                Nombre = "Ana Perez",
                Rol = "Firmante",
                Fecha = "2026-10-29T12:33:20",
                Aprobado = true
            }
        };
        var repo = new Mock<ISolicitudRestauracionFlujoQueryRepository>();
        repo.Setup(r => r.ObtenerPuntosRestauracion(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(puntos);

        var resultado = await new ObtenerPuntosRestauracionQueryHandler(new Mock<IMapper>().Object, repo.Object)
            .Handle(new ObtenerPuntosRestauracionQuery(8), CancellationToken.None);

        resultado.Should().BeSameAs(puntos);
        repo.Verify(r => r.ObtenerPuntosRestauracion(8, It.IsAny<CancellationToken>()), Times.Once);
    }
}
