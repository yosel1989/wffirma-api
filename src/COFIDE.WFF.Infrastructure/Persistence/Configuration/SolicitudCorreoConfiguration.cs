using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudCorreoConfiguration : AuditableEntityConfiguration<SolicitudCorreo, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudCorreo> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_CORREO");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_CORREO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD_CORREO");

        builder.Property(u => u.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();

        builder.OwnsOne(x => x.Correo, correo =>
        {
            correo.Property(c => c.Value)
                  .HasColumnName("VC_CORREO")
                  .HasMaxLength(50)
                  .IsRequired();
        });
    }
}
