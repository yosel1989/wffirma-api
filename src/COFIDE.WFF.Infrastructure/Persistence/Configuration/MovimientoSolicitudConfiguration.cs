using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.ValueObjects;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class MovimientoSolicitudConfiguration : AuditableEntityConfiguration<MovimientoSolicitud, long>
{
    public override void Configure(EntityTypeBuilder<MovimientoSolicitud> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_MOVIMIENTO_SOLICITUD");

        builder.HasKey(a => a.Id)
               .HasName("PK_MOVIMIENTO_SOLICITUD");

        builder.Property(x => x.Id)
               .HasColumnName("ID_MOVIMIENTO_SOLICITUD");

        builder.Property(x => x.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired(true);

        builder.Property(x => x.IdEstadoSolicitud)
               .HasColumnName("ID_ESTADO_SOLICITUD")
               .IsRequired(true);

        builder.Property(x => x.IdUsuarioEstado)
               .HasColumnName("ID_USUARIO_ESTADO")
               .IsRequired(true);

        builder.Property(x => x.FechaEstado)
               .HasColumnName("FE_FECHA_ESTADO")
               .IsRequired(true);

        builder.OwnsOne(
                x => x.MotivoCambio,
                vo =>
                {
                    vo.Property(p => p.Valor)
                      .HasColumnName("VC_MOTIVO_CAMBIO")
                      .HasMaxLength(MotivoCambio.MaxLongitud)
                      .IsRequired(false);
                });

        builder.HasOne(s => s.UsuarioEstado)
               .WithMany()
               .HasForeignKey(s => s.IdUsuarioEstado)
               .HasConstraintName("FK_SOLICITUD_MOVIMIENTO_EST")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
