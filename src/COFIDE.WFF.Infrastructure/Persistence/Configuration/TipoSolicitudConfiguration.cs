using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class TipoSolicitudConfiguration : AuditableEntityConfiguration<TipoSolicitud, long>
{
    public override void Configure(EntityTypeBuilder<TipoSolicitud> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_TIPO_SOLICITUD");

        builder.HasKey(a => a.Id)
               .HasName("PK_TIPO_SOLICITUD");

        builder.Property(x => x.Id)
               .HasColumnName("ID_TIPO_SOLICITUD");

        builder.Property(x => x.NombreTipoSolicitud)
               .HasColumnName("VC_NOMBRE_TIPO_SOLICITUD")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.DescripcionCorta)
            .HasColumnName("VC_DESCRIPCION_CORTA")
            .IsRequired();

        builder.Property(x => x.IdArea)
               .HasColumnName("ID_AREA")
               .IsRequired();

        builder.HasOne(a => a.Area)
           .WithMany()
           .HasForeignKey(a => a.IdArea)
           .HasConstraintName("FK_TIPO_SOL_AREA")
           .OnDelete(DeleteBehavior.Restrict);
    }
}
