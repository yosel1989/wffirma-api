using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudPlantillaConfiguration : AuditableEntityConfiguration<SolicitudPlantilla, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudPlantilla> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_PLANTILLA");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_PLANTILLA");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD_PLANTILLA");

        builder.Property(x => x.IdPlantilla)
               .HasColumnName("ID_PLANTILLA")
               .IsRequired();

        builder.Property(x => x.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();


        builder.HasOne(sp => sp.Solicitud)
               .WithOne(s => s.SolicitudPlantilla)
               .HasForeignKey<SolicitudPlantilla>(sp => sp.IdSolicitud)
               .HasConstraintName("FK_ETAPA_SOL_SOLICITUD")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Plantilla)
               .WithMany()
               .HasForeignKey(sp => sp.IdPlantilla)
               .HasConstraintName("FK_PLANTILLA_SOLICITUD")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.IdSolicitud)
               .IsUnique();
    }
}