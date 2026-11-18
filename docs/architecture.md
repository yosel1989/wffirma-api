# Arquitectura y flujo

## Proyectos y dependencias

- `Api.csproj` referencia Application e Infrastructure; contiene controladores, contratos HTTP, configuración y middleware (`src/COFIDE.WFF.Api/`).
- `Application.csproj` referencia Domain; contiene casos de uso, validadores, behaviors y puertos (`src/COFIDE.WFF.Application/`).
- `Infrastructure.csproj` referencia Application y Domain; contiene adaptadores, repositorios y persistencia (`src/COFIDE.WFF.Infrastructure/`).
- `Domain.csproj` no declara referencias a otros proyectos (`src/COFIDE.WFF.Domain/COFIDE.WFF.Domain.csproj`).
- `Worker.csproj` referencia Application e Infrastructure. `Worker/Program.cs` registra `ActualizarEstadoProgramadoWorker` como hosted service y `AddWindowsService`.

## Petición API

1. `Api/Program.cs` establece `/API-WFF`, registra `CorrelationMiddleware` antes de `ExceptionHandlingMiddleware`, y configura autenticación, autorización, CORS y controladores.
2. El middleware de correlación toma el header configurado o `HttpContext.TraceIdentifier`, lo devuelve en la respuesta y lo añade al contexto de Serilog (`Api/Middleware/CorrelationMiddleware.cs`).
3. Los controladores envían commands/queries a MediatR. `BaseApiController.Send` es un helper de algunas acciones; otros controladores llaman directamente a `_mediator.Send`.
4. `Application/DependencyInjection.cs` registra handlers, validadores y los behaviors `ValidationBehavior`, `AuditingBehavior`, `AuthenticatedUserBehavior` y `TransactionBehavior`.
5. `AuthenticatedUserBehavior` deja pasar tipos `IAllowAnonymousRequest`; para los demás requiere usuario autenticado y `UserIdLocal` (`Application/Common/Behaviors/AuthenticatedUserBehavior.cs`).
6. El handler usa interfaces de `Application/Common/Ports/Out`; Infrastructure registra sus implementaciones, incluido `WorkflowFirmaDbContext` con Oracle.
7. `TransactionBehavior` solo abre transacción para tipos cuyo nombre termina en `Command`; hace dispatch pre-commit, commit, confirma archivos y publica eventos post-commit. Al fallar, revierte transacción y cambios de archivos.

## Referencias

- Composición HTTP: `src/COFIDE.WFF.Api/Program.cs`.
- Behaviors y servicios de aplicación: `src/COFIDE.WFF.Application/DependencyInjection.cs`.
- Adaptadores y Oracle: `src/COFIDE.WFF.Infrastructure/DependencyInjection.cs`.
- Unit of work: `src/COFIDE.WFF.Infrastructure/Adapters/Repositories/Base/UnitOfWork.cs`.
- Contexto y auditoría: `src/COFIDE.WFF.Infrastructure/Persistence/Contexts/WorkflowFirmaDbContext.cs`.
- Worker: `src/COFIDE.WFF.Worker/Program.cs`.
