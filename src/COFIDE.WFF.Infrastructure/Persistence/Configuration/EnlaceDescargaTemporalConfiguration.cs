using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class EnlaceDescargaTemporalConfiguration : AuditableEntityConfiguration<EnlaceDescargaTemporal, long>
{
    public override void Configure(EntityTypeBuilder<EnlaceDescargaTemporal> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_ENLACE_DESCARGA_TEMPORAL");

        builder.Property(x => x.Id)
               .HasColumnName("ID_ENLACE_TEMPORAL")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.Id)
               .HasColumnName("ID_ENLACE_TEMPORAL");

        builder.Property(x => x.Token)
               .HasColumnName("VC_TOKEN")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();

        builder.Property(x => x.FechaCreada)
               .HasColumnName("FE_FECHA_CREADA")
               .IsRequired();

        builder.Property(x => x.FechaExpiracion)
               .HasColumnName("FE_FECHA_EXPIRACION")
               .IsRequired();

        builder.Property(x => x.DescargasRealizadas)
               .HasColumnName("INT_DESCARGAS_REALIZADAS")
               .IsRequired();

        builder.Property(x => x.DescargasMaximas)
               .HasColumnName("INT_DESCARGAS_MAXIMAS")
               .IsRequired(false);      
    }
}
