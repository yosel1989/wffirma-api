using COFIDE.WFF.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;

public abstract class AuditableEntityConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity> where TKey : struct
       where TEntity : AuditableEntity<TKey>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {

        builder.Property(e => e.Estado)
              .HasColumnName("CH_ESTADO")
              .HasColumnType("CHAR(1)")
              .HasConversion(
                   v => v ? "A" : "I",
                   v => v.Trim() == "A"
               )
              .IsRequired();

        // Auditoría
        builder.Property(a => a.UsuarioCreacion)
               .HasColumnName("VC_USUARIO_CREACION")
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(a => a.FechaCreacion)
               .HasColumnName("TS_FECHA_CREACION")
               .HasColumnType("TIMESTAMP")
               .HasConversion(
                    v => v.Kind == DateTimeKind.Local
                        ? v
                        : v.ToUniversalTime(),               // C# → Oracle
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local) // Oracle → C#
                    )
               .IsRequired();

        builder.Property(a => a.TerminalCreacion)
               .HasColumnName("VC_TERMINAL_CREACION")
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(a => a.UsuarioModificacion)
               .HasColumnName("VC_USUARIO_MODIFICACION")
               .HasMaxLength(30).IsRequired(false);

        builder.Property(a => a.FechaModificacion)
               .HasColumnName("TS_FECHA_MODIFICACION")
               .HasColumnType("TIMESTAMP")
               .HasConversion(
                    v => v == null
                            ? (DateTime?)null
                            : (v.Value.Kind == DateTimeKind.Local
                                ? v.Value
                                : v.Value.ToUniversalTime()),        // C# → Oracle
                    v => v == null
                            ? (DateTime?)null
                            : DateTime.SpecifyKind(v.Value, DateTimeKind.Local) // Oracle → C#
                )
               .IsRequired(false);

        builder.Property(a => a.TerminalModificacion)
               .HasColumnName("VC_TERMINAL_MODIFICACION")
               .HasMaxLength(30).IsRequired(false);

     
       
    }
}
