using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Factories;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Inputs;

public class AccesoEspecialFactoryTests
{
    [Fact]
    public void AccesoEspecialRegistrar_CreaInstanciaCorrecta()
    {
        // Arrange
        var input = new AccesoEspecialRegistrarInput
        {
            IdUsuario = 123,
            IdArea = 456
        };

        // Act
        var result = AccesoEspecialFactory.AccesoEspecialRegistrar(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.IdUsuario, result.IdUsuario);
        Assert.Equal(input.IdArea, result.IdArea);
        Assert.Equal(0, result.Id); // El factory asigna 0 como id inicial
    }
}