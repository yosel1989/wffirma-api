# WFF - API Hexagonal

API del Workflow de Firmas, desarrollada bajo arquitectura Hexagonal (Ports & Adapters). Registra
solicitudes, arma su flujo de aprobación a partir de plantillas, coordina la firma de documentos (interna y
externa) y notifica a los involucrados.

## Descripción

Esta API actúa como orquestador entre:

- **SSO** (proveedor de autenticación): emite el token JWT con el que se autentican los usuarios.
- **API Suc** (menú y datos externos): además entrega la configuración en tiempo de ejecución (cadena de conexión, clave JWT) y los datos del usuario.
- **API de firma digital (FIRD)**: firma de documentos y callback con el resultado.
- **API de correo** y **Hub de sockets**: notificaciones por correo y en tiempo real.
- **Oracle**: base de datos (esquema `US_WFFIRMAS`).

## Stack

- .NET 8 / ASP.NET Core
- EF Core con Oracle.EntityFrameworkCore
- MediatR (CQRS), FluentValidation, AutoMapper, Scrutor
- Serilog, Swashbuckle
- xUnit, Moq, FluentAssertions, NetArchTest

## Estructura

```
src/
  COFIDE.WFF.Domain           Agregados (Solicitud, Plantilla), entidades, value objects, eventos y servicios de dominio
  COFIDE.WFF.Application      Commands, Queries y Handlers (MediatR), puertos, validaciones, DTOs
  COFIDE.WFF.Infrastructure   Adaptadores: EF/Oracle, repositorios, APIs externas, archivos, seguridad
  COFIDE.WFF.Api              Controladores, contratos HTTP (Requests), middleware, Swagger
  COFIDE.WFF.Worker           Procesos programados (actualización de estados)
tests/
  COFIDE.WFF.Domain.Tests
  COFIDE.WFF.Application.Tests
  COFIDE.WFF.Infrastructure.Tests
  COFIDE.WFF.Api.Tests
  COFIDE.WFF.Api.IntegrationTests
  COFIDE.WFF.Architecture.Tests
```

## Arquitectura

Usamos arquitectura hexagonal (puertos y adaptadores) con CQRS sobre MediatR. La regla principal es que las
dependencias siempre apuntan hacia el dominio:

```
Api / Worker  ->  Infrastructure  ->  Application  ->  Domain
```

- **Domain**
  - Entidades y agregados
  - Value Objects
  - Eventos y servicios de dominio
  - No referencia ningún otro proyecto ni paquete. Aquí van las reglas de negocio.

- **Application**
  - Casos de uso (Commands / Queries y Handlers de MediatR)
  - Puertos (interfaces) que implementa Infrastructure
  - DTOs y validaciones
  - No conoce ASP.NET, EF Core ni HTTP.

- **Infrastructure**
  - Adaptadores externos (SUC, firma, correo, hub de sockets)
  - Repositorios (EF Core / Oracle)
  - Configuración de seguridad (JWT) y almacenamiento de archivos

- **API**
  - Controllers y contratos HTTP (Requests)
  - Configuración Swagger
  - Middleware

- **Worker**: procesos programados.

`COFIDE.WFF.Architecture.Tests` verifica estas reglas; si un cambio las rompe, falla al correr las pruebas.

### Recorrido de una petición

1. El controlador recibe un **Request** (contrato HTTP, en `Api/Contracts/Requests`).
2. Lo convierte en un **Command** o **Query** con los métodos de `Api/Mappings` (`request.ToCommand(id)`).
3. MediatR lo envía al **Handler**, pasando por los behaviors de validación, usuario autenticado y transacción.
4. El handler carga el agregado, ejecuta la operación del dominio y guarda.
5. `TransactionBehavior` confirma la transacción y luego publica los eventos.

### Convenciones

- **Commands y Queries** terminan en `Command` / `Query`. Un handler por caso de uso; los handlers no se llaman entre sí.
- **Requests solo en la Api.** Application no tiene clases `*Request` ni tipos de ASP.NET.
- **Archivos:** la Api convierte `IFormFile` en `ArchivoEntrada`, que es lo único que llega a Application.
  Las rutas y la estructura de carpetas (`DOCUMENTOS`, `SUSTENTOS`) las resuelve `IFileStorageService`; el ZIP lo arma `IZipStreamingService`.
- **Repositorios:**
  - `I*Repository` carga y guarda agregados.
  - `I*QueryRepository` es el lado de lectura y devuelve DTOs listos para mostrar (listados, bandeja, panel).
- **Dominio:** las entidades no exponen setters públicos; el estado cambia con métodos del agregado.
  La paginación y los DTOs no pertenecen al dominio (están en `Application/Common/Pageds`).
- **Eventos de dominio:**
  - *Pre-commit*: se procesan dentro de la transacción.
  - *Post-commit* (correos, historial, tiempo real): se publican apenas se confirma la transacción. Si alguno
    falla, la petición no devuelve error y el evento se reintenta en segundo plano (sección `PublicacionEventos`).
- **Archivos y transacciones:** si una transacción se revierte, los archivos escritos durante ella se eliminan
  y los sobrescritos se restauran.

## Configuración

`src/COFIDE.WFF.Api/appsettings.json`:

| Sección | Para qué |
|---|---|
| `ExternalServices` | URLs y códigos de sistema del SUC, correo, firma y hub de sockets |
| `FileServer:BaseUrl` | Ruta base donde se guardan los documentos |
| `FrontEnd` / `BackEnd` | URLs usadas para armar los enlaces de los correos |
| `JwtSettings` | Validación del token del SSO |
| `CorsOptions`, `RateLimiting` | CORS y límite de peticiones |
| `SerilogOptions` | Rutas y nivel de los logs |
| `PublicacionEventos` | Reintentos de eventos post-commit: `MaximoReintentos`, `SegundosEntreReintentos`, `CapacidadCola` |

La cadena de conexión a Oracle y la clave JWT **no** están en el archivo: se obtienen del SUC al iniciar
(`ExternalServices:SucWCF`).

> Por ahora las credenciales de desarrollo están en `appsettings.json`. Está previsto pasarlas a un gestor de secretos.

## Ejecución

```bash
dotnet restore
dotnet build
dotnet run --project src/COFIDE.WFF.Api
dotnet run --project src/COFIDE.WFF.Worker
```

- La Api se publica bajo el path base `/API-WFF`. Las rutas son versionadas: `/API-WFF/api/v1/...`
- Swagger: `/API-WFF/swagger`
## Autenticación

La API utiliza autenticación basada en JWT.

El token es emitido por el SSO y debe enviarse en el header:

```
Authorization: Bearer <token>
```

Con el token, la API obtiene del SUC los datos del usuario (área, cargo, rol) que usan los casos de uso.
La descarga pública por token (`/api/v1/public/{token}`) es la única ruta que no requiere autenticación.

## Pruebas

| Proyecto | Qué cubre | Requiere BD |
|---|---|---|
| Domain.Tests | Agregados, value objects, servicios de dominio | No |
| Application.Tests | Handlers, casos de uso y behaviors (con Moq) | No |
| Infrastructure.Tests | Publicación de eventos, almacenamiento de archivos, ZIP | No |
| Api.Tests | Componentes de la Api | No |
| Architecture.Tests | Reglas de dependencias, DDD, CQRS y nomenclatura | No |
| Api.IntegrationTests | Repositorios contra Oracle, endpoints de punta a punta, SUC | Sí |

```bash
# Pruebas rápidas (sin base de datos ni servicios externos)
dotnet test --filter "Categoria!=Integracion&Categoria!=ApisExternas"

# Integración
dotnet test tests/COFIDE.WFF.Api.IntegrationTests
```

**Estructura de los proyectos de prueba:**

```
Tests/       clases de prueba, agrupadas por funcionalidad
Support/     helpers sin asserts (Object Mothers, dobles, JsonResource...)
Resources/   datos de prueba en JSON
```

Los archivos y clases de prueba se nombran en singular: `FirmaUseCaseTest`, `SolicitudConsultaHandlerTest`.

**Pruebas de integración.** Copia
`tests/COFIDE.WFF.Api.IntegrationTests/Resources/Configuracion/conexiones.example.json` como `conexiones.json`
y completa los datos de la base de **desarrollo**. Ese archivo está en `.gitignore`. Si no existe, las pruebas
de integración se omiten. En el pipeline se pueden usar variables de entorno con prefijo `WFF_IT_`.

**Pruebas de arquitectura.** Si una regla falla por deuda conocida que se corregirá después, se puede
registrar en `tests/COFIDE.WFF.Architecture.Tests/Support/LineaBase.cs` (la prueba imprime el bloque a copiar).
A partir de ahí solo falla si aparece algo nuevo.

## Pendientes técnicos

- Unificar la regla de "turno del aprobador" (hoy está en dos consultas que no son iguales) y reemplazar los roles escritos como texto.
- Quitar del dominio la bandera `EsNuevo` y la generación anticipada de Ids.
- Manejo centralizado de errores en los handlers de correo e historial (hoy capturan sus excepciones).
- Reemplazar la publicación de eventos en memoria por colas.
- Mover las credenciales a un gestor de secretos.
