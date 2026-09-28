using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class ParametroCabeceraConfiguration
    : AuditableEntityConfiguration<ParametroCabecera, long>
{
    public override void Configure(EntityTypeBuilder<ParametroCabecera> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_PARAMETRO_CABECERA");

        builder.HasKey(a => a.Id)
               .HasName("PK_WFF_PARAMETRO_CABECERA");

        builder.Property(x => x.Id)
               .HasColumnName("ID_PARAMETRO_CABECERA");

        builder.Property(x => x.CodigoParametro)
               .HasColumnName("VC_CODIGO_PARAMETRO");

        builder.Property(x => x.DescripcionParametro)
               .HasColumnName("VC_DESCRIPCION_PARAMETRO");
    }
}