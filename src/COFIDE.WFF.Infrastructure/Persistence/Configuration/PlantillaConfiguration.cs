using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Vo;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class PlantillaConfiguration : AuditableEntityConfiguration<Plantilla, long>
{
    public override void Configure(EntityTypeBuilder<Plantilla> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_PLANTILLA");

        builder.HasKey(a => a.Id)
               .HasName("PK_PLANTILLA_FLUJO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_PLANTILLA");

        builder.OwnsOne(
          x => x.NombrePlantilla,
          vo =>
          {
              vo.Property(p => p.Valor)
                  .HasColumnName("VC_NOMBRE_PLANTILLA")
                  .HasMaxLength(NombrePlantilla.MaxLongitud)
                  .IsRequired(false);
          });

        builder.OwnsOne(
         x => x.Descripcion,
         vo =>
         {
             vo.Property(p => p.Valor)
                 .HasColumnName("VC_DESCRIPCION")
                 .HasMaxLength(DescripcionPlantilla.MaxLongitud)
                 .IsRequired(false);
         });

        builder.Property(u => u.Visibilidad)
             .HasColumnName("CH_VISIBILIDAD")
             .HasColumnType("CHAR(1)")
             .HasConversion(
                 v => v.Value ? "S" : "N",
                 v => v.Trim() == "S"
             )
             .IsRequired(false);

        builder.Property(x => x.IdUsuarioPropietario)
               .HasColumnName("ID_USUARIO_PROPIETARIO")
               .IsRequired(false);

        builder.Property(x => x.GerenciaUsuario)
             .HasColumnName("VC_GERENCIA_USUARIO")
             .HasMaxLength(100)
             .IsRequired(false);

        builder.Property(u => u.EsReutilizable)
             .HasColumnName("CH_ES_REUTILIZABLE")
             .HasColumnType("CHAR(1)")
             .HasConversion(
                 v => v.Value ? "S" : "N",
                 v => v.Trim() == "S"
             )
             .IsRequired(false);

      /*  builder.HasOne(e => e.UsuarioPropietario)
                .WithMany()
                .HasForeignKey(e => e.IdUsuarioPropietario)
                .HasConstraintName("FK_PLANTILLA_USUARIO")
                .OnDelete(DeleteBehavior.Restrict);*/
    }
}
