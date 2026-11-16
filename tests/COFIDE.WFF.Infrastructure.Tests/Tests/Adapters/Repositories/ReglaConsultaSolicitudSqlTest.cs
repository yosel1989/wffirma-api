using COFIDE.WFF.Domain.DomainServices;
using COFIDE.WFF.Infrastructure.Persistence.Contexts;
using COFIDE.WFF.Infrastructure.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace COFIDE.WFF.Infrastructure.Tests.Tests.Adapters.Repositories;

public class ReglaConsultaSolicitudSqlTest
{
    [Theory]
    [InlineData("COORD", false)]
    [InlineData("COORD", true)]
    [InlineData("SOLAPR", false)]
    [InlineData("SOLAPR", true)]
    [InlineData("ADMIN", false)]
    [InlineData("ADMIN", true)]
    [InlineData(null, false)]
    [InlineData(null, true)]
    public void Predicado_Debe_Traducirse_A_Oracle_Antes_De_Paginar_Y_Contar(string? rol, bool accesoEspecial)
    {
        var opciones = new DbContextOptionsBuilder<WorkflowFirmaDbContext>()
            .UseOracle("Data Source=sin-conexion")
            .Options;
        using var contexto = new WorkflowFirmaDbContext(opciones, new CurrentUserService(new HttpContextAccessor()));
        long[] areas = accesoEspecial ? [5, 50] : [];
        var visibles = contexto.Solicitudes
            .Where(ReglaConsultaSolicitud.CrearPredicado(20, 99, rol, areas));

        // ToQueryString solo traduce; no abre una conexion ni ejecuta SQL.
        var pagina = visibles.OrderBy(s => s.Id).Select(s => s.Id).Skip(10).Take(10).ToQueryString();
        var resumen = visibles.GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.LongCount(),
                Urgente = g.LongCount(s => s.EsUrgente),
                MisSolicitudes = g.LongCount(s => s.IdUsuarioSolicitante == 20)
            }).ToQueryString();

        pagina.Should().Contain("SELECT").And.Contain("OFFSET").And.Contain("FETCH NEXT");
        resumen.Should().Contain("COUNT(");
        pagina.Should().NotMatchRegex(@"\b(FALSE|TRUE)\b");
        resumen.Should().NotMatchRegex(@"\b(FALSE|TRUE)\b");
        if (rol is "COORD" or "SOLAPR")
        {
            pagina.Should().Contain("WHERE");
            resumen.Should().Contain("WHERE");
            if (accesoEspecial)
            {
                pagina.Should().Contain(" IN ").And.Contain(" OR ")
                    .And.Contain("ID_AREA_SOLICITUD").And.Contain("ID_AREA_PADRE_SOLI");
                resumen.Should().Contain(" IN ").And.Contain(" OR ")
                    .And.Contain("ID_AREA_SOLICITUD").And.Contain("ID_AREA_PADRE_SOLI");
            }
        }
        else if (rol is null)
        {
            pagina.Should().Contain("WHERE");
            resumen.Should().Contain("WHERE");
        }
    }
}
