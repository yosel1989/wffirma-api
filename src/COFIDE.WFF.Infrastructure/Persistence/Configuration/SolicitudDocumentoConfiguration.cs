using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudDocumentoConfiguration : AuditableEntityConfiguration<SolicitudDocumento, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudDocumento> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_DOCUMENTO");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_DOCUMENTO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD_DOCUMENTO");

        builder.Property(x => x.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")           
               .IsRequired();

        builder.Property(u => u.NombreOriginal)
               .HasColumnName("VC_NOMBRE_ORIGINAL")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(u => u.ExtensionOriginal)
               .HasColumnName("VC_EXTENSION_ORIGINAL")
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(u => u.HashOriginal)
               .HasColumnName("VC_HASH_ORIGINAL")
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(u => u.RutaOriginal)
              .HasColumnName("VC_RUTA_ORIGINAL")
              .HasMaxLength(500)
              .IsRequired();

        builder.Property(u => u.NombreCodigo)
              .HasColumnName("VC_NOMBRE_CODIGO")
              .HasMaxLength(255)
              .IsRequired(false);

        builder.Property(x => x.ExtensionCodigo)
               .HasColumnName("VC_EXTENSION_CODIGO")
               .HasMaxLength(10)
               .IsRequired(false);

        builder.Property(u => u.RutaCodigo)
              .HasColumnName("VC_RUTA_CODIGO")
              .HasMaxLength(500)
              .IsRequired(false);

        builder.Property(u => u.HashCodigo)
             .HasColumnName("VC_HASH_CODIGO")
             .HasMaxLength(500)
             .IsRequired(false);

        builder.Property(u => u.NombreConTag)
             .HasColumnName("VC_NOMBRE_CONTAG")
             .HasMaxLength(255)
             .IsRequired(false);

        builder.Property(x => x.ExtensionConTag)
               .HasColumnName("VC_EXTENSION_CONTAG")
               .HasMaxLength(10)
               .IsRequired(false);

        builder.Property(u => u.RutaConTag)
              .HasColumnName("VC_RUTA_CONTAG")
              .HasMaxLength(500)
              .IsRequired(false);

        builder.Property(u => u.HashConTag)
             .HasColumnName("VC_HASH_CONTAG")
             .HasMaxLength(500)
             .IsRequired(false);

        builder.Property(u => u.PesoDocumento)
             .HasColumnName("INT_PESO_DOCUMENTO");

        builder.HasOne(sd => sd.Solicitud)
            .WithMany(s => s.Documentos)
            .HasForeignKey(sd => sd.IdSolicitud)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sd => sd.DetallesFirma)
           .WithOne(df => df.SolicitudDocumento)
           .HasForeignKey(df => df.IdSolicitudDocumento)
           .OnDelete(DeleteBehavior.Cascade);

    }
}
