using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
//namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

//internal class SolicitudFirmanteFlujoConfiguration : AuditableEntityConfiguration<SolicitudFirmanteFlujo, long>
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration
{
    public class SolicitudFirmanteFlujoConfiguration
        : AuditableEntityConfiguration<SolicitudFirmanteFlujo, long>
    {
        public override void Configure(EntityTypeBuilder<SolicitudFirmanteFlujo> builder)
        {
            base.Configure(builder);

            builder.ToTable("WFF_SOLICITUD_FIRMANTE_FLUJO");

            builder.HasKey(x => x.Id)
               .HasName("PK_SOLICITUD_FIRMANTE_FLUJO");

            builder.Property(x => x.Id)
                   .HasColumnName("ID_SOLI_FIRMANTE_FLUJO")
                   .ValueGeneratedNever();

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

            builder.HasOne(a => a.SolicitudPlantilla)
                   .WithMany(s => s.FirmantesFlujo)
                   .HasForeignKey(a => a.IdSolicitudPlantilla)
                   .HasConstraintName("FK_SOLI_PLANT_FIRMANTE_FLUJO")
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FirmanteFlujo)
                   .WithMany()
                   .HasForeignKey(x => x.IdFirmanteFlujo)
                   .HasConstraintName("FK_FIRMANTE_FLUJO_SOLI")
                   .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(x => x.SolicitudPlantilla)
            //       .WithMany()
            //       .HasForeignKey(x => x.IdSolicitudPlantilla)
            //       .HasConstraintName("FK_SOLI_PLANT_FIRMANTE_FLUJO");

            //builder.HasOne(x => x.PlantillaEtapa)
            //       .WithMany()
            //       .HasForeignKey(x => x.IdPlantillaEtapa)
            //       .HasConstraintName("FK_PLANT_ETAPA_FIRMANTE")
            //       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}