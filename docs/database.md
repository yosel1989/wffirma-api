# Base de datos

## Contexto Oracle

`WorkflowFirmaDbContext` establece esquema por defecto `US_WFFIRMAS`, define los `DbSet` y aplica `IEntityTypeConfiguration` del ensamblado. Infrastructure obtiene la conexión de `IRuntimeConfigurationProvider`, usa `UseOracle` y fija timeout de comando en 60 segundos (`src/COFIDE.WFF.Infrastructure/DependencyInjection.cs`).

## Tablas configuradas explícitamente

Los 30 `ToTable("WFF_...")` están en `Infrastructure/Persistence/Configuration/`:

- Catálogos: `WFF_USUARIO`, `WFF_AREA`, `WFF_CARGO`, `WFF_TIPO_SOLICITUD`, `WFF_SUBTIPO_SOLICITUD`, `WFF_TIPO_OPERACION`, `WFF_ESTADO_SOLICITUD`, `WFF_ESTADO_DOCUMENTO`, `WFF_TIPO_EVENTO`.
- Solicitudes: `WFF_SOLICITUD`, `WFF_SOLICITUD_DOCUMENTO`, `WFF_SOLICITUD_SUSTENTO`, `WFF_SOLICITUD_PLANTILLA`, `WFF_SOLICITUD_FIRMANTE_FLUJO`, `WFF_SOLICITUD_COD_REFERENCIA`, `WFF_SOLICITUD_COMENTARIO`, `WFF_SOLICITUD_CORREO`, `WFF_SOLICITUD_EVENTO`, `WFF_MOVIMIENTO_SOLICITUD`.
- Plantillas y firma: `WFF_PLANTILLA`, `WFF_PLANTILLA_ETAPA`, `WFF_FIRMANTE_FLUJO`, `WFF_DETALLE_FIRMA`, `WFF_DOCUMENTO_TAG`, `WFF_HISTORIAL_REVISION`.
- Otros: `WFF_PARAMETRO_CABECERA`, `WFF_PARAMETRO_DETALLE`, `WFF_FORMATO_NOTIFICACION`, `WFF_ENLACE_DESCARGA_TEMPORAL`, `WFF_ACCESO_ESPECIAL`.

## Columnas y auditoría

Las configuraciones asignan nombres explícitos con `HasColumnName` a muchas columnas; usan mayúsculas y no siguen un prefijo uniforme:

- `ID_`: claves/referencias; `VC_`: cadenas; `CH_`: indicadores CHAR(1). Por ejemplo `WFF_SOLICITUD.CH_ES_URGENTE` mapea `bool` a `S`/`N`.
- `FE_` aparece en fechas de solicitud; `TS_` en fechas de auditoría. `CL_MENSAJE` se configura como `CLOB`; `INT_PESO_TOTAL_DOCU`, como `NUMBER(12,0)` (`src/COFIDE.WFF.Infrastructure/Persistence/Configuration/SolicitudConfiguration.cs`).
- La configuración común mapea estado (`A`/`I`), usuario, terminal y fecha de creación/modificación (`src/COFIDE.WFF.Infrastructure/Persistence/Configuration/Base/AuditableEntityConfiguration.cs`).
- `SaveChangesAsync` aplica auditoría a entidades `AuditableEntity<long>` nuevas y modificadas. Si falta usuario/IP, usa `SYSTEM` y `::1` (`src/COFIDE.WFF.Infrastructure/Persistence/Contexts/WorkflowFirmaDbContext.cs`).
- Ejemplo de relaciones: documento -> solicitud usa `Restrict`; detalles de firma del documento usan `Cascade` (`src/COFIDE.WFF.Infrastructure/Persistence/Configuration/SolicitudDocumentoConfiguration.cs`).
- El código de `OnModelCreating` solo convierte nombres a snake case y mayúsculas cuando no hay nombre explícito; los nombres `WFF_*` ya contienen `_` y se conservan.

## Referencias

- Contexto, esquema y auditoría: `src/COFIDE.WFF.Infrastructure/Persistence/Contexts/WorkflowFirmaDbContext.cs`.
- Tablas y columnas: `src/COFIDE.WFF.Infrastructure/Persistence/Configuration/`.
- Columnas de solicitud: `src/COFIDE.WFF.Infrastructure/Persistence/Configuration/SolicitudConfiguration.cs`.
- Columnas de documento: `src/COFIDE.WFF.Infrastructure/Persistence/Configuration/SolicitudDocumentoConfiguration.cs`.
- Conexión Oracle: `src/COFIDE.WFF.Infrastructure/DependencyInjection.cs`.
