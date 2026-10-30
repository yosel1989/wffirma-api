using Moq;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Queries.ObtenerAccesoEspecialPaginado;
using COFIDE.WFF.Application.Features.AccesosEspeciales.Ports.Out;
using COFIDE.WFF.Application.Common.Pageds;
using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Application.Common.Errors;
using COFIDE.WFF.Application.Features.AccesosEspeciales.DTOs;

public class ObtenerAccesoEspecialPaginadoQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _userMock;
    private readonly Mock<IAccesoEspecialQueryRepository> _repoMock;
    private readonly ObtenerAccesoEspecialPaginadoQueryHandler _handler;

    public ObtenerAccesoEspecialPaginadoQueryHandlerTests()
    {
        _userMock = new Mock<ICurrentUserService>();
        _repoMock = new Mock<IAccesoEspecialQueryRepository>();
        _handler = new ObtenerAccesoEspecialPaginadoQueryHandler(_userMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task Handle_UsuarioAutenticadoYConCargo_RetornaDto()
    {
        // Arrange
        _userMock.SetupGet(u => u.UserIdLocal).Returns(10);
        _userMock.SetupGet(u => u.UserCargoId).Returns(20);

        var query = new ObtenerAccesoEspecialPaginadoQuery
        (1,1)
        {
            PageNumber = 1,
            PageSize = 10,
            IdArea = 1,
            IdUsuario = 2,
            SearchTerm = "test"
        };

        var pagedItems = new List<AccesoEspecialPaginadoDto>
        {
            new AccesoEspecialPaginadoDto { IdAccesoEspecial = 1, IdUsuario = 2, IdArea = 1 }
        };
        var resumen = new AccesoEspecialResumenPaginadoDto { Total = 1 };
        var pagedResult = new PagedResult<AccesoEspecialPaginadoDto, AccesoEspecialResumenPaginadoDto>(
            pagedItems, 1, 1, 10)
        {
            ExtraData = resumen
        };

        _repoMock.Setup(r => r.ObtenerPaginado(
            It.IsAny<PagedQueryParams>(),
            query.IdArea,
            query.IdUsuario,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resumen.Total, result.Total);
        Assert.Equal(pagedItems, result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalCount);
    }


}
