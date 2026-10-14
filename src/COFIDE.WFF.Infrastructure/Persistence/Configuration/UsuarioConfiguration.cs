using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class UsuarioConfiguration : AuditableEntityConfiguration<Usuario, long>
{
    public override void Configure(EntityTypeBuilder<Usuario> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_USUARIO");

        builder.HasKey(a => a.Id)
               .HasName("PK_USUARIO");

        builder.Property(x => x.Id)
               .HasColumnName("ID_USUARIO");

        builder.Property(x => x.Nombres)
               .HasColumnName("VC_NOMBRES")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.ApellidoPaterno)
               .HasColumnName("VC_APELLIDO_PATERNO")
               .HasMaxLength(80)
               .IsRequired();

        builder.Property(x => x.ApellidoMaterno)
               .HasColumnName("VC_APELLIDO_MATERNO")
               .HasMaxLength(80)
               .IsRequired();

        builder.Property(x => x.NumeroDocumento)
               .HasColumnName("VC_NUMERO_DOCUMENTO")
               .HasMaxLength(20)
               .IsRequired(false);

        builder.Property(x => x.Email)
               .HasColumnName("VC_EMAIL")
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.CuentaLogueo)
               .HasColumnName("VC_CUENTA_LOGUEO")
               .HasMaxLength(50)
               .IsRequired(false);

        builder.Property(u => u.IdArea)
               .HasColumnName("ID_AREA");

        builder.Property(u => u.IdCargo)
               .HasColumnName("ID_CARGO");

        builder.Property(u => u.Notificacion)
              .HasColumnName("CH_NOTIFICACION")
              .HasColumnType("CHAR(1)")
              .IsRequired(false); 


        //Claves foraneas

        builder.HasOne(a => a.Area)
               .WithMany()
               .HasForeignKey(a => a.IdArea)
               .HasConstraintName("FK_USUARIO_AREA")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Cargo)
            .WithMany()
            .HasForeignKey(a => a.IdCargo)
            .HasConstraintName("FK_USUARIO_CARGO")
            .OnDelete(DeleteBehavior.Restrict);

    }
}
