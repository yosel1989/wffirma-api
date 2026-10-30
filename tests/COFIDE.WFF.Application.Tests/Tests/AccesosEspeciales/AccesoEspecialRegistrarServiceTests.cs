using COFIDE.WFF.Application.Features.AccesosEspeciales.Services;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;


public class AccesoEspecialRegistrarServiceTests
{
    [Fact]
    public void AccesoEspecialRegistrar_DevuelveEntidadCorrecta()
    {
        // Arrange
        var service = new AccesoEspecialRegistrarService();
        var dto = new AccesoEspecialRequestDto
        {
            IdUsuario = 123,
            IdArea = 456
        };

        // Mock del factory (si es necesario, dependiendo de la implementación real)
        // Aquí asumimos que AccesoEspecialFactory.AccesoEspecialRegistrar funciona correctamente.

        // Act
        var result = service.AccesoEspecialRegistrar(dto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.IdUsuario, result.IdUsuario);
        Assert.Equal(dto.IdArea, result.IdArea);
    }
}