using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration
{
    public class HistorialRevisionConfiguration : AuditableEntityConfiguration<HistorialRevision, long>
    {
        public override void Configure(EntityTypeBuilder<HistorialRevision> builder)
        {
            base.Configure(builder);

            builder.ToTable("WFF_HISTORIAL_REVISION");

            builder.HasKey(x => x.Id)
               .HasName("PK_HISTORIAL_REVISION");

            builder.Property(x => x.Id)
                   .HasColumnName("ID_HISTORIAL_REVISION")
                   .ValueGeneratedNever();

            builder.Property(x => x.IdSolFirmanteFlujo)
                      .HasColumnName("ID_SOLI_FIRMANTE_FLUJO")
                      .IsRequired();

            builder.Property(x => x.IdFirmanteFlujo)
                      .HasColumnName("ID_FIRMANTE_FLUJO")
                      .IsRequired();

            builder.Property(x => x.IdSolicitudPlantilla)
                   .HasColumnName("ID_SOLICITUD_PLANTILLA")
                   .IsRequired();

            builder.Property(x => x.IdPlantillaEtapa)
                   .HasColumnName("ID_PLANTILLA_ETAPA")
                   .IsRequired();

            builder.Property(x => x.RevAprobado)
               .HasColumnName("CH_REV_APROBADO")
               .HasColumnType("CHAR(1)")
               .HasConversion(
                   v => v ? "S" : "N",
                   v => v.Trim() == "S"
               )
               .IsRequired();

            builder.Property(x => x.FechaRevision)
                   .HasColumnName("TS_FECHA_REVISION");

            builder.Property(x => x.Ciclo)
                   .HasColumnName("NU_CICLO");

            builder.HasOne(b => b.SolFirmanteFlujo)
                   .WithMany(s=>s.HistRevision)
                   .HasForeignKey(b => b.IdSolFirmanteFlujo)
                   .HasConstraintName("FK_SOLI_FIRMANTE_FLUJO_HISTORIAL_REVISION")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmanteFlujo)
                  .WithMany()
                  .HasForeignKey(x => x.IdFirmanteFlujo)
                  .HasConstraintName("FK_FIRMANTE_FLUJO_HISTORIAL_REVISION")
                  .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.SolicitudPlantilla)
                   .WithMany()
                   .HasForeignKey(a => a.IdSolicitudPlantilla)
                   .HasConstraintName("FK_SOLI_PLANT_HISTORIAL_REVISION")
                   .OnDelete(DeleteBehavior.Restrict);

           
        }
    }
}
