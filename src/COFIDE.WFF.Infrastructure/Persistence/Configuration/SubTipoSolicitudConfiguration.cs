using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SubTipoSolicitudConfiguration : AuditableEntityConfiguration<SubTipoSolicitud, long>
{
    public override void Configure(EntityTypeBuilder<SubTipoSolicitud> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SUBTIPO_SOLICITUD");

        builder.HasKey(a => a.Id)
           .HasName("PK_SUBTIPO_SOLICITUD");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SUBTIPO_SOLICITUD");

        builder.Property(x => x.IdTipoSolicitud)
              .HasColumnName("ID_TIPO_SOLICITUD")
              .IsRequired();

        builder.Property(x => x.NombreSubTipoSolicitud)
               .HasColumnName("VC_NOMBRE_SUBTIPO_SOL")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.IdArea)
             .HasColumnName("ID_AREA");

        builder.HasOne(a => a.Area)
           .WithMany()
           .HasForeignKey(a => a.IdArea)
           .HasConstraintName("FK_SUBTIPO_SOL_AREA")
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TipoSolicitud)
          .WithMany()
          .HasForeignKey(a => a.IdTipoSolicitud)
          .HasConstraintName("FK_SUBTIPO_TIPO_SOL")
          .OnDelete(DeleteBehavior.Restrict);
    }
}
