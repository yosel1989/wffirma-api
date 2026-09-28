using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class DocumentoTagConfiguration : AuditableEntityConfiguration<DocumentoTag, long>
{
    public override void Configure(EntityTypeBuilder<DocumentoTag> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_DOCUMENTO_TAG");

        builder.HasKey(a => a.Id)
               .HasName("PK_WFF_DOCUMENTO_TAG");

        builder.Property(x => x.Id)
               .HasColumnName("ID_DOCUMENTO_TAG");

        builder.Property(x => x.IdSolicitudDocumento)
               .HasColumnName("ID_SOLICITUD_DOCUMENTO")
               .IsRequired();

        builder.Property(x => x.IdDetalleFirma)
               .HasColumnName("ID_DETALLE_FIRMA");            

        builder.Property(x => x.NumeroPagina)
               .HasColumnName("INT_NUMERO_PAGINA");

        builder.Property(x => x.UbicacionX)
               .HasColumnName("INT_UBICACION_X")
               .HasColumnType("NUMBER(18,15)");

        builder.Property(x => x.UbicacionY)
               .HasColumnName("INT_UBICACION_Y")
               .HasColumnType("NUMBER(18,15)");

        builder.Property(x => x.AnchoFirma)
               .HasColumnName("INT_ANCHO_FIRMA")
               .HasColumnType("NUMBER(10,2)");

        builder.Property(x => x.AltoFirma)
               .HasColumnName("INT_ALTO_FIRMA")
               .HasColumnType("NUMBER(10,2)");

        // Referencias
        builder.HasOne(df => df.SolicitudDocumento)
               .WithMany(sd => sd.DocumentoTags)
               .HasForeignKey(df => df.IdSolicitudDocumento)
               .HasConstraintName("FK_SOLICITUD_TAG_DOCUMENTO")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
