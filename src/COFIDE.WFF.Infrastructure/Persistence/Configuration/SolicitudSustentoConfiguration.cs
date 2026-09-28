using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudSustentoConfiguration : AuditableEntityConfiguration<SolicitudSustento, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudSustento> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_SUSTENTO");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_SUSTENTO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD_SUSTENTO");

        builder.Property(x => x.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();

        builder.Property(u => u.NombreSustento)
               .HasColumnName("VC_NOMBRE_SUSTENTO")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(u => u.ExtensionSustento)
               .HasColumnName("VC_EXTENSION_SUSTENTO")
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(u => u.RutaSustento)
               .HasColumnName("VC_RUTA_SUSTENTO")
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(u => u.PesoSustento)
             .HasColumnName("INT_PESO_SUSTENTO");
    }
}
