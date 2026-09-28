using COFIDE.WFF.Application.Common.Ports.Out;
using COFIDE.WFF.Domain.Aggregates.PlantillaAggregate;
using COFIDE.WFF.Domain.Aggregates.SolicitudAggregate;
using COFIDE.WFF.Domain.Common.Entities;
using COFIDE.WFF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace COFIDE.WFF.Infrastructure.Persistence.Contexts;

public class WorkflowFirmaDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;
    // Constructor que permite la inyección de opciones de configuración desde DI.
    public WorkflowFirmaDbContext(DbContextOptions<WorkflowFirmaDbContext> options, ICurrentUserService currentUser)
    : base(options) { _currentUser = currentUser; }

    // DbSets expuestos para las entidades agregadas relevantes en este contexto.
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<SolicitudPlantilla> SolicitudPlantillas => Set<SolicitudPlantilla>();
    public DbSet<FormatoNotificacion> FormatoNotificaciones => Set<FormatoNotificacion>();
    public DbSet<Plantilla> Plantillas => Set<Plantilla>();
    public DbSet<PlantillaEtapa> PlantillaEtapas => Set<PlantillaEtapa>();
    public DbSet<FirmanteFlujo> FirmanteFlujos => Set<FirmanteFlujo>();
    public DbSet<DetalleFirma> DetalleFirmas => Set<DetalleFirma>();
    public DbSet<TipoSolicitud> TipoSolicitudes => Set<TipoSolicitud>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<SubTipoSolicitud> SubTipoSolicitud => Set<SubTipoSolicitud>();
    public DbSet<TipoOperacion> TipoOperacion => Set<TipoOperacion>();
    public DbSet<SolicitudCodReferencia> SolicitudesCodReferencia => Set<SolicitudCodReferencia>();
    public DbSet<ParametroCabecera> ParametroCabeceras => Set<ParametroCabecera>();
    public DbSet<ParametroDetalle> ParametroDetalles => Set<ParametroDetalle>();
    public DbSet<SolicitudDocumento> SolicitudDocumentos => Set<SolicitudDocumento>();
    public DbSet<SolicitudSustento> SolicitudSustentos => Set<SolicitudSustento>();
    public DbSet<SolicitudFirmanteFlujo> SolicitudFirmanteFlujos => Set<SolicitudFirmanteFlujo>();
    public DbSet<EstadoDocumento> EstadoDocumento => Set<EstadoDocumento>();
    public DbSet<EstadoSolicitud> EstadoSolicitud => Set<EstadoSolicitud>();
    public DbSet<EnlaceDescargaTemporal> EnlaceDescargaTemporales => Set<EnlaceDescargaTemporal>();


    /// <summary>
    /// Configuración del modelo durante la creación del contexto.
    /// - Establece el esquema por defecto 'CUSTOMER'.
    /// - Aplica todas las configuraciones de entidades automáticamente desde el ensamblado.
    /// - Convierte todos los nombres de tablas y columnas a snake_case de forma automática.
    /// </summary>  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("US_WFFIRMAS"); // Esquema por defecto

        // Aplica todas las configuraciones desde los assemblies (Solicitud, SolicitudDocumento, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowFirmaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);

        // Loop seguro para aplicar snake_case solo donde no hay HasColumnName explícito
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.IsOwned())
                continue;

            // No tocar tablas que ya tienen guion bajo o todas mayúsculas
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName) && (tableName.Contains("_") || tableName.All(char.IsUpper)))
                continue;

            // Aplica snake_case a nombre de tabla si no tiene _
            entity.SetTableName(ToSnakeCase(tableName!));

            foreach (var property in entity.GetProperties())
            {
                // Obtiene el nombre de columna tal como EF lo tiene en la metadata
                var storeObjectId = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
                var columnName = property.GetColumnName(storeObjectId);

                // Solo aplica snake_case si no hay HasColumnName explícito
                if (string.IsNullOrEmpty(columnName) || columnName == property.Name)
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }
        }

        // Ignorar el ValueObject TipoEtapa, ya que es solo para validaciones y no debe persistir a la bd     

        modelBuilder.Entity<SolicitudPlantilla>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudFirmanteFlujo>().Ignore(e => e.OrdenEtapa);
        modelBuilder.Entity<SolicitudFirmanteFlujo>().Ignore(e => e.IdUsuarioAsignado);
        modelBuilder.Entity<SolicitudFirmanteFlujo>().Ignore(e => e.EsFirmante);
        modelBuilder.Entity<SolicitudFirmanteFlujo>().Ignore(e => e.EsVisador);
        modelBuilder.Entity<SolicitudFirmanteFlujo>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudDocumento>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudSustento>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<DetalleFirma>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudCodReferencia>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudComentario>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudCorreo>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<MovimientoSolicitud>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<SolicitudEvento>().Ignore(e => e.EsNuevo);

        modelBuilder.Entity<Plantilla>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<PlantillaEtapa>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<PlantillaEtapa>().Ignore(e => e.TempId);
        modelBuilder.Entity<PlantillaEtapa>().Ignore(e => e.TipoEtapa);
        modelBuilder.Entity<PlantillaEtapa>().Ignore(e => e.TipoAprobador);
        modelBuilder.Entity<FirmanteFlujo>().Ignore(e => e.EsNuevo);
        modelBuilder.Entity<DocumentoTag>().Ignore(e => e.EsNuevo);
    }

    /// <summary>
    /// Método utilitario para convertir nombres PascalCase a snake_case.
    /// Ejemplo: "SujetoRegistrado" => "sujeto_registrado"
    /// </summary>
    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;

        return string.Concat(
            name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())
        ).ToUpper();
    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity<long>>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreationAudit(
                    _currentUser?.CodigoUsuario ?? "SYSTEM",
                    _currentUser?.IpAddress ?? "::1");
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdateAudit(
                    _currentUser?.CodigoUsuario ?? "SYSTEM",
                    _currentUser?.IpAddress ?? "::1");
            }
        }
    }
}
