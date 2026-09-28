using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class FormatoNotificacionConfiguration : AuditableEntityConfiguration<FormatoNotificacion, long>
{
    public override void Configure(EntityTypeBuilder<FormatoNotificacion> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_FORMATO_NOTIFICACION");

        builder.HasKey(a => a.Id)
               .HasName("PK_PLANTILLA_NOTIFICACION");

        builder.Property(x => x.Id)
               .HasColumnName("ID_FORMATO_NOTIFICACION");

        builder.Property(x => x.NombreFormato)
               .HasColumnName("VC_NOMBRE_FORMATO")
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.CorreoRemitente)
               .HasColumnName("VC_CORREO_REMITENTE")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.NombreRemitente)
               .HasColumnName("VC_NOMBRE_REMITENTE")
               .HasMaxLength(255)
               .IsRequired(false);

        builder.Property(x => x.CorreoDestinatario)
               .HasColumnName("VC_CORREO_DESTINATARIO")
               .HasMaxLength(255)
               .IsRequired(false);

        builder.Property(x => x.CopiaDestinatario)
               .HasColumnName("VC_COPIA_DESTINATARIO")
               .HasMaxLength(255)
               .IsRequired(false);

        builder.Property(x => x.AsuntoCorreo)
               .HasColumnName("VC_ASUNTO_CORREO")
               .HasMaxLength(250)
               .IsRequired(false);

        builder.Property(x => x.CuerpoMensaje)
               .HasColumnName("CL_CUERPO_MENSAJE")
               .HasColumnType("CLOB")
               .IsRequired(false);
    }
}
