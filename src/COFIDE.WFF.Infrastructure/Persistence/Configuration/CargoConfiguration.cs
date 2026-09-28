using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class CargoConfiguration : AuditableEntityConfiguration<Cargo, long>
{
    public override void Configure(EntityTypeBuilder<Cargo> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_CARGO");

        builder.HasKey(a => a.Id)
               .HasName("PK_CARGO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_CARGO");

        builder.Property(x => x.CodigoCargo)
               .HasColumnName("VC_CODIGO_CARGO")
               .HasMaxLength(4)
               .IsRequired(false);

        builder.Property(x => x.NombreCargo)
               .HasColumnName("VC_NOMBRE_CARGO")
               .HasMaxLength(100)
               .IsRequired();

    }
}