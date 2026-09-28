using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class TipoEventoConfiguration : AuditableEntityConfiguration<TipoEvento, long>
{
    public override void Configure(EntityTypeBuilder<TipoEvento> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_TIPO_EVENTO");

        builder.HasKey(a => a.Id)
               .HasName("PK_TIPO_EVENTO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_TIPO_EVENTO");

        builder.Property(u => u.NombreEvento)
               .HasColumnName("VC_NOMBRE_EVENTO")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(u => u.Descripcion)
               .HasColumnName("VC_DESCRIPCION")
               .HasMaxLength(200);

        builder.Property(x => x.FormatoMensaje)
               .HasColumnName("VC_FORMATO_MENSAJE")
               .HasMaxLength(150);

        builder.Property(u => u.RequiereDestino)
               .HasColumnName("CH_REQUIERE_DESTINO")
               .HasColumnType("CHAR(1)")
               .HasConversion(
                   v => v.Value ? "S" : "N",
                   v => v.Trim() == "S"
               )
               .IsRequired(false);
    }
}