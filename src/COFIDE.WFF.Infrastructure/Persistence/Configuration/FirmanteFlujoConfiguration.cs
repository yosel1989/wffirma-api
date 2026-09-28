using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class FirmanteFlujoConfiguration : AuditableEntityConfiguration<FirmanteFlujo, long>
{
    public override void Configure(EntityTypeBuilder<FirmanteFlujo> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_FIRMANTE_FLUJO");

        builder.HasKey(a => a.Id)
               .HasName("PK_FIRMANTE_FLUJO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_FIRMANTE_FLUJO");

        builder.Property(x => x.IdPlantillaEtapa)
               .HasColumnName("ID_PLANTILLA_ETAPA");

        builder.Property(x => x.IdUsuarioAsignado)
               .HasColumnName("ID_USUARIO_ASIGNADO");

        builder.Property(x => x.CargoUsuario)
               .HasColumnName("VC_CARGO_USUARIO")
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.AreaUsuario)
               .HasColumnName("VC_AREA_USUARIO")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.GerenciaUsuario)
               .HasColumnName("VC_GERENCIA_USUARIO")
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(x => x.FechaAsignacion)
              .HasColumnName("TS_FECHA_ASIGNACION");

        builder.Property(u => u.EsSuplente)
             .HasColumnName("CH_ES_SUPLENTE")
             .HasColumnType("CHAR(1)")
             .HasConversion(
                 v => v ? "S" : "N",
                 v => v.Trim() == "S"
             )
             .IsRequired();

        builder.Property(u => u.EsDelegado)
             .HasColumnName("CH_ES_DELEGADO")
             .HasColumnType("CHAR(1)")
             .HasConversion(
                 v => v ? "S" : "N",
                 v => v.Trim() == "S"
             )
             .IsRequired();     


        builder.Property(x => x.IdUsuarioOrigen)
            .HasColumnName("ID_USUARIO_ORIGEN");


        builder.HasOne(a => a.PlantillaEtapa)
           .WithMany(s => s.Firmantes)
           .HasForeignKey(a => a.IdPlantillaEtapa)
           .HasConstraintName("FK_PLANTILLA_ETAPA_FIRMANTE")
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FirmanteFlujoUsuarioAsignado)
                 .WithMany()
                 .HasForeignKey(e => e.IdUsuarioAsignado)
                 .HasConstraintName("FK_FIRMANTE_FLUJO_USUARIO")
                 .OnDelete(DeleteBehavior.Restrict);

    }
}

