using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;
public class SolicitudComentarioConfiguration : AuditableEntityConfiguration<SolicitudComentario, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudComentario> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_COMENTARIO");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_COMENTARIO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLI_COMENTARIO")
               .ValueGeneratedNever();

        builder.Property(u => u.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();

        builder.Property(u => u.IdUsuario)
               .HasColumnName("ID_USUARIO")
               .IsRequired();

        builder.Property(x => x.Comentario)
               .HasColumnName("VC_COMENTARIO")
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.FechaComentario)
              .HasColumnName("FE_FECHA_COMENTARIO")
              .IsRequired();

        builder.HasOne(a => a.Solicitud)
               .WithMany(s => s.Comentarios)
               .HasForeignKey(a => a.IdSolicitud)
               .HasConstraintName("FK_SOLICITUD_COMENTARIO_SOL")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Usuario)
               .WithMany()
               .HasForeignKey(s => s.IdUsuario)
               .HasConstraintName("FK_SOLICITUD_USUARIO_SOL")
               .OnDelete(DeleteBehavior.Restrict);
    }
}