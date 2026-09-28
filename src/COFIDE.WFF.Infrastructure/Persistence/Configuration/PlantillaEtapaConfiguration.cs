using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class PlantillaEtapaConfiguration : AuditableEntityConfiguration<PlantillaEtapa, long>
{
    public override void Configure(EntityTypeBuilder<PlantillaEtapa> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_PLANTILLA_ETAPA");

        builder.HasKey(a => a.Id)
               .HasName("PK_PLANTILLA_ETAPA");

        builder.Property(x => x.Id)
               .HasColumnName("ID_PLANTILLA_ETAPA");

        builder.Property(x => x.IdPlantilla)
               .HasColumnName("ID_PLANTILLA");

        builder.Property(x => x.OrdenEtapa)
             .HasColumnName("NM_ORDEN_ETAPA");

        builder.OwnsOne(
          x => x.NombreEtapa,
          vo =>
          {
              vo.Property(p => p.Valor)
                  .HasColumnName("VC_NOMBRE_ETAPA")
                  .HasMaxLength(NombreEtapa.MaxLongitud)
                  .IsRequired();
          });

        builder.Property(x => x.IdTipoEtapa)
              .HasColumnName("ID_TIPO_ETAPA")
              .IsRequired();

        builder.Property(x => x.IdTipoAprobador)
               .HasColumnName("ID_TIPO_APROBADOR")
               .IsRequired();

        builder.HasOne(a => a.Plantilla)
              .WithMany(s => s.Etapas)
              .HasForeignKey(a => a.IdPlantilla)
              .HasConstraintName("FK_PLANTILLA_PLANTILLA_ETAPA")
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ParametroTipoAprobador)
               .WithMany()
               .HasForeignKey(e => e.IdTipoAprobador)
               .HasConstraintName("FK_ETAPA_PLANT_TIPO")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ParametroTipoEtapa)
               .WithMany()
               .HasForeignKey(e => e.IdTipoEtapa)
               .HasConstraintName("FK_PLANTILLA_ETAPA_DEPARAMETRO")
               .OnDelete(DeleteBehavior.Restrict);


    }
}
