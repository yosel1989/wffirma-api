using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class DetalleFirmaConfiguration : AuditableEntityConfiguration<DetalleFirma, long>
{
    public override void Configure(EntityTypeBuilder<DetalleFirma> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_DETALLE_FIRMA");

        builder.Property(x => x.Id)
               .HasColumnName("ID_DETALLE_FIRMA")
               .ValueGeneratedNever();

        builder.Property(x => x.Id)
               .HasColumnName("ID_DETALLE_FIRMA");

        builder.Property(x => x.IdSolicitudFirmanteFlujo)
               .HasColumnName("ID_SOLI_FIRMANTE_FLUJO")
               .IsRequired();

        builder.Property(x => x.IdSolicitudDocumento)
               .HasColumnName("ID_SOLICITUD_DOCUMENTO")
               .IsRequired();

        builder.Property(x => x.NombreDocumentoFirmado)
               .HasColumnName("VC_NOMBRE_DOCUMENTO_FIRMADO")
               .HasMaxLength(150)
               .IsRequired(false);

        builder.Property(x => x.ExtensionDocumentoFirmado)
              .HasColumnName("VC_EXTENSION_DOCU_FIRMADO")
              .HasMaxLength(10)
              .IsRequired(false);

        builder.Property(x => x.RutaDocumentoFirmado)
               .HasColumnName("VC_RUTA_DOCUMENTO_FIRMADO")
               .HasMaxLength(250)
               .IsRequired(false);

        builder.Property(x => x.HashDocumentoFirmado)
               .HasColumnName("VC_HASH_DOCU_FIRMADO")
               .HasMaxLength(500)
               .IsRequired(false);

        builder.Property(x => x.IdFirmanteDocumento)
               .HasColumnName("ID_FIRMANTE_DOCUMENTO")
               .IsRequired(false);

        builder.Property(x => x.FechaHoraFirma)
               .HasColumnName("TS_FECHA_HORA_FIRMA");

        builder.Property(x => x.InternoExterno)
            .HasConversion(
                v => v != null ? v.Codigo : (char?)null,
                v => v.HasValue ? InternoExterno.FromCodigo(v.Value) : null
            )
            .HasColumnName("CH_INTERNO_EXTERNO")
            .IsRequired(false);

        builder.Property(x => x.EstadoDocumento)
               .HasConversion(
                   v => v.Codigo,
                   v => EstadoDocumento.FromCodigo(v)
               )
               .HasColumnName("ID_ESTADO_DOCUMENTO")
               .IsRequired(false);

        builder.Property(x => x.FechaInicioProceso)
               .HasColumnName("FE_INICIO_PROCESO");

        builder.Property(x => x.FechaFinEsperado)
               .HasColumnName("FE_FIN_ESPERADO");

        builder.Property(x => x.CodigoFirma)
               .HasColumnName("VC_CODIGO_FIRMA")
               .HasMaxLength(200);

        builder.Property(x => x.CodigoReferencial)
               .HasColumnName("VC_CODIGO_REFERENCIAL")
               .HasMaxLength(20);

        // Referencias
        builder.HasOne(df => df.SolicitudDocumento)
               .WithMany(sd => sd.DetallesFirma)
               .HasForeignKey(df => df.IdSolicitudDocumento)
               .HasConstraintName("FK_SOLICITUD_DOCUMENTO_DETALLE")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(df => df.SolicitudFirmanteFlujo)
               .WithMany()
               .HasForeignKey(df => df.IdSolicitudFirmanteFlujo)
               .HasConstraintName("FK_FIRMA_FLUJO_DETALLE")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.UsuarioFirmante)
           .WithMany()
           .HasForeignKey(s => s.IdFirmanteDocumento)
           .HasConstraintName("FK_DETALLE_FIRMA_USU")
           .OnDelete(DeleteBehavior.Restrict);
    }
}