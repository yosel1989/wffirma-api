using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;
public class AreaConfiguration : AuditableEntityConfiguration<Area, long>
{
    public override void Configure(EntityTypeBuilder<Area> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_AREA");

        builder.HasKey(a => a.Id)
               .HasName("PK_AREA");

        builder.Property(x => x.Id)
               .HasColumnName("ID_AREA");

        builder.Property(x => x.CodDependencia)
               .HasColumnName("VC_COD_DEPENDENCIA")
               .HasMaxLength(5)
               .IsRequired(false);

        builder.Property(x => x.NombreArea)
               .HasColumnName("VC_NOMBRE_AREA")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.DescripcionCorta)
              .HasColumnName("VC_DESCRIPCION_CORTA")
              .HasMaxLength(12)
              .IsRequired();

        builder.Property(x => x.Tipo)
               .HasColumnName("CH_TIPO")
               .HasColumnType("CHAR(1)")
               .IsRequired();

        builder.Property(u => u.IdAreaPadre)
               .HasColumnName("ID_AREA_PADRE")
               .IsRequired(false);

        builder.HasOne(a => a.AreaPadre)
               .WithMany(a => a.SubAreas)
               .HasForeignKey(a => a.IdAreaPadre)
               .HasConstraintName("FK_AREA_PADRE")
               .OnDelete(DeleteBehavior.Restrict);

    }
}
