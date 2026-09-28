using COFIDE.WFF.Domain.Entities;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration
{
    public class TipoOperacionConfiguration : AuditableEntityConfiguration<TipoOperacion, long>
    {
        public override void Configure(EntityTypeBuilder<TipoOperacion> builder)
        {
            base.Configure(builder);

            builder.ToTable("WFF_TIPO_OPERACION");

            builder.Property(x => x.Id)
                .HasColumnName("ID_TIPO_OPERACION");

            builder.Property(x => x.NombreTipoOperacion)
                .HasColumnName("VC_NOMBRE_TIPO_OPERACION");

            builder.Property(x => x.IdArea)
                .HasColumnName("ID_AREA");

            builder.HasOne(a => a.Area)
              .WithMany()
              .HasForeignKey(a => a.IdArea)
              .HasConstraintName("FK_TIPO_OPERACION_AREA")
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
