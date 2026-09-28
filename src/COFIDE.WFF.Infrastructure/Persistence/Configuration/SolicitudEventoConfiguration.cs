using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudEventoConfiguration : AuditableEntityConfiguration<SolicitudEvento, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudEvento> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_EVENTO");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_EVENTO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD_EVENTO");

        builder.Property(x => x.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();

        builder.Property(x => x.IdTipoEvento)
               .HasColumnName("ID_TIPO_EVENTO")
               .IsRequired();

        builder.Property(x => x.IdUsuarioOrigen)
               .HasColumnName("ID_USUARIO_ORIGEN")
               .IsRequired();

        builder.Property(x => x.IdUsuarioDestino)
               .HasColumnName("ID_USUARIO_DESTINO");

        builder.Property(x => x.IdPlantillaEtapa)
               .HasColumnName("ID_PLANTILLA_ETAPA");

        builder.Property(x => x.AreaUsuario)
               .HasColumnName("VC_AREA_USUARIO");

        builder.Property(x => x.IdSolicitudSustento)
               .HasColumnName("ID_SOLICITUD_SUSTENTO");

        builder.Property(x => x.FechaEvento)
               .HasColumnName("FE_FECHA_EVENTO");

        builder.HasOne(a => a.Solicitud)
               .WithMany(s => s.Eventos)
               .HasForeignKey(a => a.IdSolicitud)
               .HasConstraintName("FK_SOLICITUD_EVENTO_SOL")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TipoEvento)
               .WithMany()
               .HasForeignKey(a => a.IdTipoEvento)
               .HasConstraintName("FK_SOLICITUD_EVENTO_TIPO")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UsuarioOrigen)
              .WithMany()
              .HasForeignKey(a => a.IdUsuarioOrigen)
              .HasConstraintName("FK_EVENTO_USUARIO_ORIGEN")
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UsuarioDestino)
              .WithMany()
              .HasForeignKey(a => a.IdUsuarioDestino)
              .HasConstraintName("FK_EVENTO_USUARIO_DESTINO")
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.SolicitudSustento)
      .WithMany()
      .HasForeignKey(a => a.IdSolicitudSustento)
      .HasConstraintName("FK_EVENTO_SOLICITUD_SUSTENTO")
      .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.PlantillaEtapa)
             .WithMany()
             .HasForeignKey(a => a.IdPlantillaEtapa)             
             .OnDelete(DeleteBehavior.Restrict);
    }
}
