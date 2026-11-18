# Convenciones observadas

## Nombres y organización

- Controladores HTTP terminan en `Controller` y se ubican en `src/COFIDE.WFF.Api/Controllers/v1/`.
- Application se agrupa por feature y por `Commands`/`Queries`. Las operaciones usan tipos `*Command`, `*Query`, `*Handler` y `*Validator`.
- Interfaces de salida se ubican en `Application/Common/Ports/Out/` o en `Features/<feature>/Ports/Out/`; sus adaptadores se registran en Infrastructure.
- Errores de aplicación: campos `static readonly ApplicationError` en `Application/Common/Errors/ApplicationErrors.cs`. Errores de dominio: `static readonly DomainError` por agregado en `Domain/Aggregates/*/Errors/`.
- `Api/Constants/AppMessages.cs` contiene mensajes `const string`; `Api/Constants/ResponseCodes.cs` contiene códigos semánticos de respuesta. Son catálogos distintos de `ApplicationErrors`.
- Configuración EF: clases `*Configuration` bajo `Infrastructure/Persistence/Configuration/`.

## Pruebas

- Hay seis proyectos bajo `tests/`: Domain, Application, Infrastructure, Api, Api.IntegrationTests y Architecture; los seis usan xUnit.
- Los ejemplos organizan pruebas bajo `Tests/<área>/`; clases suelen terminar en `Test` (p. ej. `Domain.Tests/Tests/Solicitudes/ReglaConsultaSolicitudTest.cs`).
- El código usa `[Fact]`, `[Theory]` e `[InlineData]`; en Domain.Tests se observan aserciones FluentAssertions.
- Infrastructure.Tests separa pruebas por adaptador, por ejemplo `Tests/Adapters/Repositories/ReglaConsultaSolicitudSqlTest.cs`.
- Los `.csproj` fijan referencias de cada suite; Api.Tests referencia Api y Application. IntegrationTests incluye recursos JSON y un README propio.

## Referencias

- Ejemplo command/handler/validator: `src/COFIDE.WFF.Application/Features/AccesosEspeciales/Commands/RegistrarAccesoEspecial/`.
- Constantes: `src/COFIDE.WFF.Api/Constants/AppMessages.cs`, `ResponseCodes.cs`.
- Errores: `src/COFIDE.WFF.Application/Common/Errors/ApplicationErrors.cs`, `src/COFIDE.WFF.Domain/Aggregates/SolicitudAggregate/Errors/SolicitudErrors.cs`.
- Proyectos de prueba y ejemplo de aserciones: `tests/COFIDE.WFF.Domain.Tests/`, `tests/COFIDE.WFF.Infrastructure.Tests/`.
