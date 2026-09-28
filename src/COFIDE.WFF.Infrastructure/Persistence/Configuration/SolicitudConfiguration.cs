using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Infrastructure.Persistence.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COFIDE.WFF.Infrastructure.Persistence.Configuration;

public class SolicitudConfiguration : AuditableEntityConfiguration<Solicitud, long>
{
    public override void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        base.Configure(builder);

        builder.ToTable("WFF_SOLICITUD");

        builder.HasKey(a => a.Id)
               .HasName("PK_SOLICITUD");

        builder.Property(x => x.Id)
               .HasColumnName("ID_SOLICITUD")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.CodigoSolicitud)
               .HasColumnName("VC_CODIGO_SOLICITUD")
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(u => u.IdTipoSolicitud)
               .HasColumnName("ID_TIPO_SOLICITUD")
               .IsRequired();

        builder.Property(u => u.IdSubTipoSolicitud)
               .HasColumnName("ID_SUBTIPO_SOLICITUD")
               .IsRequired();

        builder.Ignore(x => x.EstadoSolicitud);

        builder.Property(x => x.IdEstadoSolicitud)
               .HasColumnName("ID_ESTADO_SOLICITUD")
               .IsRequired();

        builder.Property(u => u.IdUsuarioSolicitante)
               .HasColumnName("ID_USUARIO_SOLICITANTE")
               .IsRequired();

        builder.Property(x => x.FechaSolicitud)
               .HasColumnName("FE_FECHA_SOLICITUD")
               .IsRequired();

        builder.Property(u => u.IdTipoOperacion)
               .HasColumnName("ID_TIPO_OPERACION")
               .IsRequired();

        builder.Property(u => u.IdUsuarioAccion)
               .HasColumnName("ID_USUARIO_ACCION")
               .IsRequired();

        builder.Property(u => u.IdAreaSolicitud)
               .HasColumnName("ID_AREA_SOLICITUD")
               .IsRequired();

        builder.Property(u => u.IdAreaPadreSolicitud)
               .HasColumnName("ID_AREA_PADRE_SOLI")
               .IsRequired();

        builder.Property(x => x.FechaAccion)
               .HasColumnName("FE_FECHA_ACCION")
               .IsRequired();

        builder.Property(x => x.Asunto)
               .HasColumnName("VC_ASUNTO")
               .HasMaxLength(200)
               .IsRequired(false);

        builder.Property(x => x.Mensaje)
               .HasColumnName("CL_MENSAJE")
               .HasColumnType("CLOB")
               .IsRequired(false);

        builder.Property(u => u.EsUrgente)
               .HasColumnName("CH_ES_URGENTE")
               .HasColumnType("CHAR(1)")
               .HasConversion(
                   v => v ? "S" : "N",
                   v => v.Trim() == "S")
               .IsRequired();

        builder.Property(u => u.InCodigo)
               .HasColumnName("CH_IN_CODIGO")
               .HasColumnType("CHAR(1)")
               .HasConversion(
                   v => v ? "S" : "N",
                   v => v.Trim() == "S")
               .IsRequired();

        builder.OwnsOne(x => x.FechaLimite, fecha =>
        {
            fecha.Property(x => x.Valor)
                .HasColumnName("FE_FECHA_LIMITE");
        });

        builder.Property(x => x.MotivoRechazo)
               .HasColumnName("VC_MOTIVO_RECHAZO")
               .HasMaxLength(350)
               .IsRequired(false);

        builder.Property(x => x.PesoTotalDocu)
               .HasColumnName("INT_PESO_TOTAL_DOCU")
               .HasColumnType("NUMBER(12,0)")
               .IsRequired(false);

        builder.HasOne(s => s.TipoSolicitud)
               .WithMany()
               .HasForeignKey(s => s.IdTipoSolicitud)
               .HasConstraintName("FK_SOLICITUD_TIPO")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SubTipoSolicitud)
               .WithMany()
               .HasForeignKey(s => s.IdSubTipoSolicitud)
               .HasConstraintName("FK_SOLICITUD_SUBTIPO")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TipoOperacion)
               .WithMany()
               .HasForeignKey(s => s.IdTipoOperacion)
               .HasConstraintName("FK_SOLICITUD_TIPO_OPERACION")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.UsuarioSolicitante)
               .WithMany()
               .HasForeignKey(s => s.IdUsuarioSolicitante)
               .HasConstraintName("FK_SOLICITUD_SOLICITANTE")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Documentos)
               .WithOne()
               .HasForeignKey(d => d.IdSolicitud)
               .HasConstraintName("FK_SOLICITUD_DOCUMENTO_SOLICITUD")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Sustentos)
               .WithOne()
               .HasForeignKey(d => d.IdSolicitud)
               .HasConstraintName("FK_SOLICITUD_SUSTENTO_SOL")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Correos)
               .WithOne()
               .HasForeignKey(d => d.IdSolicitud)
               .HasConstraintName("FK_SOLICITUD_CORREO_SOL")
               .OnDelete(DeleteBehavior.Restrict);
    }
}