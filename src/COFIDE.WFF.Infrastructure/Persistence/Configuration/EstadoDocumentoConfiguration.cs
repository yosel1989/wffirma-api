using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class EstadoDocumentoConfiguration : AuditableEntityConfiguration<EstadoDocumento, long>
{
    public override void Configure(EntityTypeBuilder<EstadoDocumento> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_ESTADO_DOCUMENTO");

        builder.HasKey(a => a.Id)
               .HasName("PK_ESTADO_DOCUMENTO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_ESTADO_DOCUMENTO");

        builder.Property(x => x.NombreEstadoDocumento)
               .HasColumnName("VC_NOMBRE_ESTADO_DOC")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Descripcion)
              .HasColumnName("VC_DESCRIPCION")
              .HasMaxLength(200)
              .IsRequired(false);

    }
}
