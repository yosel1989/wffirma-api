using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class ParametroDetalleConfiguration
    : AuditableEntityConfiguration<ParametroDetalle, long>
{
    public override void Configure(EntityTypeBuilder<ParametroDetalle> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_PARAMETRO_DETALLE");

        builder.HasKey(x => x.Id)
               .HasName("PK_WFF_PARAMETRO_DETALLE");

        builder.Property(x => x.Id)
               .HasColumnName("ID_PARAMETRO_DETALLE");

        builder.Property(x => x.IdParametroCabecera)
               .HasColumnName("ID_PARAMETRO_CABECERA");

        builder.Property(x => x.CodigoDetalle)
               .HasColumnName("VC_CODIGO_DETALLE");

        builder.Property(x => x.DescripcionDetalle)
               .HasColumnName("VC_DESCRIPCION_DETALLE")
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.ValorParametro)
               .HasColumnName("VC_VALOR_PARAMETRO");

        builder.HasOne(d => d.Cabecera)
               .WithMany(c => c.Detalles)
               .HasForeignKey(d => d.IdParametroCabecera)
               .HasConstraintName("FK_WFF_PARAM_DET_CAB");
    }
}