using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Vo;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class AccesoEspecialConfiguration : AuditableEntityConfiguration<AccesoEspecial, long>
{
    public override void Configure(EntityTypeBuilder<AccesoEspecial> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_ACCESO_ESPECIAL");

        builder.HasKey(a => a.Id)
               .HasName("PK_ACCESO_ESPECIAL");

        builder.Property(x => x.Id)
               .HasColumnName("ID_ACCESO_ESPECIAL")
               .ValueGeneratedOnAdd();

        builder.Property(u => u.IdUsuario)
               .HasColumnName("ID_USUARIO")
               .IsRequired();

        builder.Property(u => u.IdArea)
               .HasColumnName("ID_AREA")
               .IsRequired();

        // Conversion

        builder.Ignore(e => e.Estado);

        builder.Property(e => e.CodigoEstado)
            .HasColumnName("CH_ESTADO")
            .HasColumnType("CHAR(1)")
            .HasConversion(
                v => v.Codigo.ToString(),
                v => EstadoCodigo.FromCodigo(v.Trim()[0]))
            .IsRequired();

        // Foreign Keys

        builder.HasOne(x => x.Usuario)
           .WithMany()
           .HasForeignKey(x => x.IdUsuario)
           .HasConstraintName("FK_ACCESO_ESPECIAL_USUARIO");

        builder.HasOne(x => x.Area)
               .WithMany()
               .HasForeignKey(x => x.IdArea)
               .HasConstraintName("FK_ACCESO_ESPECIAL_AREA");

    }
}