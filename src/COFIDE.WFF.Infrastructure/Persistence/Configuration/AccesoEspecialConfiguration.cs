using COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate;
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

        builder.Property(u => u.IdGerencia)
               .HasColumnName("ID_GERENCIA")
               .IsRequired();

        builder.Property(u => u.IdDepartamento)
               .HasColumnName("ID_DEPARTAMENTO");

    }
}