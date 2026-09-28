using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Vo;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudCodReferenciaConfiguration : AuditableEntityConfiguration<SolicitudCodReferencia, long>
{
    public override void Configure(EntityTypeBuilder<SolicitudCodReferencia> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD_COD_REFERENCIA");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD_COD_REFERENCIA");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD_COD_REFERENCIA");

        builder.Property(u => u.IdSolicitud)
               .HasColumnName("ID_SOLICITUD")
               .IsRequired();

        builder.OwnsOne(
            x => x.CodigoReferencia,
            vo =>
            {
                vo.Property(p => p.Valor)
                    .HasColumnName("VC_CODIGO_REFERENCIA")
                    .HasMaxLength(CodigoReferencia.MaxLongitud)
                    .IsRequired();
            });

        builder.HasOne(a => a.Solicitud)
         .WithMany(s => s.CodigosReferencia)
         .HasForeignKey(a => a.IdSolicitud)
         .HasConstraintName("FK_SOLICITUD_COD_REFERENCIA_SOL")
         .OnDelete(DeleteBehavior.Restrict);

    }
}
