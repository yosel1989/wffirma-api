using COFIDE.WFF.Application.Common.Pageds;
using FluentAssertions;

namespace COFIDE.WFF.Application.Tests.Common;

public class PagedResultDtoTest
{
    [Fact]
    public void Create_Debe_Calcular_Paginas_Y_Navegacion()
    {
        var pagina = PagedResultDto<string>.Create(["a", "b"], 2, 10, 25);

        pagina.Items.Should().Equal("a", "b");
        pagina.TotalPages.Should().Be(3);
        pagina.HasPrevious.Should().BeTrue();
        pagina.HasNext.Should().BeTrue();
    }

    [Fact]
    public void Empty_Debe_Quedar_Sin_Elementos_Ni_Pagina_Siguiente()
    {
        var pagina = PagedResultDto<string>.Empty(1, 10);

        pagina.Items.Should().BeEmpty();
        pagina.TotalCount.Should().Be(0);
        pagina.TotalPages.Should().Be(0);
        pagina.HasPrevious.Should().BeFalse();
        pagina.HasNext.Should().BeFalse();
    }
}
