using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

    public  class EstadoSolicitudConfiguration : AuditableEntityConfiguration<EstadoSolicitud, long>
    {
    public override void Configure(EntityTypeBuilder<EstadoSolicitud> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_ESTADO_SOLICITUD");

        builder.HasKey(a => a.Id)
               .HasName("PK_ESTADO_SOLICITUD");

        builder.Property(x => x.Id)
               .HasColumnName("ID_ESTADO_SOLICITUD");

        builder.Property(x => x.NombreEstadoSolicitud)
               .HasColumnName("VC_NOMBRE_ESTADO_SOL")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Descripcion)
              .HasColumnName("VC_DESCRIPCION")
              .HasMaxLength(200)
              .IsRequired(false);

    }
}

