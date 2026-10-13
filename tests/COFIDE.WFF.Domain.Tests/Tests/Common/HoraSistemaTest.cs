using COFIDE.WFF.Domain.Common.Tiempo;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Domain.Exceptions;
using FluentAssertions;

namespace COFIDE.WFF.Domain.Tests.Tests.Common;

public class HoraSistemaTest
{
    [Fact]
    public void Fijar_Debe_Devolver_La_Fecha_Indicada_Y_Restaurarla_Al_Liberar()
    {
        var fecha = new DateTime(2026, 12, 31, 23, 59, 0);

        using (HoraSistema.Fijar(fecha))
        {
            HoraSistema.Ahora.Should().Be(fecha);
            HoraSistema.Hoy.Should().Be(fecha.Date);
        }

        HoraSistema.Ahora.Should().NotBe(fecha);
    }

    [Fact]
    public void Enlace_Debe_Estar_Disponible_Antes_De_Su_Vencimiento()
    {
        using var _ = HoraSistema.Fijar(new DateTime(2026, 1, 10, 9, 0, 0));
        var enlace = EnlaceDescargaTemporal.Crear("token", 8, new DateTime(2026, 1, 10, 18, 0, 0));

        var act = () => enlace.ValidarDisponible();

        act.Should().NotThrow();
    }

    [Fact]
    public void Enlace_Debe_Expirar_Despues_De_Su_Vencimiento()
    {
        var enlace = EnlaceDescargaTemporal.Crear("token", 8, new DateTime(2026, 1, 10, 18, 0, 0));
        using var _ = HoraSistema.Fijar(new DateTime(2026, 1, 10, 18, 0, 1));

        var act = () => enlace.ValidarDisponible();

        act.Should().Throw<DomainException>();
    }
}
