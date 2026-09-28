using COFIDE.WFF.Domain.Common.Entities;
using FluentAssertions;
namespace COFIDE.WFF.Domain.Tests.Common.Entities;
public class AuditableEntityTests
{
    [Fact]
    public void SetCreationAudit_Debe_Asignar_Datos_De_Creacion()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.SetCreationAudit("Mario", "PC01");

        // Assert
        entity.UsuarioCreacion.Should().Be("Mario");
        entity.TerminalCreacion.Should().Be("PC01");
        entity.FechaCreacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SetUpdateAudit_Debe_Asignar_Datos_De_Modificacion()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.SetUpdateAudit("Juan", "PC02");

        // Assert
        entity.UsuarioModificacion.Should().Be("Juan");
        entity.TerminalModificacion.Should().Be("PC02");
        entity.FechaModificacion.Should().NotBeNull();
        entity.FechaModificacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Activar_Debe_Cambiar_Estado_A_True()
    {
        var entity = new TestAuditableEntity();

        entity.Activar();

        entity.Estado.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_Debe_Cambiar_Estado_A_False()
    {
        var entity = new TestAuditableEntity();

        entity.Activar();

        entity.Desactivar();

        entity.Estado.Should().BeFalse();
    }

    private sealed class TestAuditableEntity : AuditableEntity<int>
    {
    }
}