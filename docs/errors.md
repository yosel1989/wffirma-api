# Errores y respuestas HTTP

## Tipos de error

- `ApplicationError` es un record (`Code`, `Description`); `Format` devuelve una copia con la descripción formateada. `ApplicationErrors` centraliza instancias (`Application/Common/Errors/`).
- `ApplicationExceptionBase` conserva un `ApplicationError`; `ApplicationException` es concreta. `ValidationException` deriva de esa base y conserva `ValidationError` (`Code`, `Message`, `Property`).
- `DomainExceptionBase` conserva un `DomainError` (`Domain/Exceptions/DomainExceptionBase.cs`). Los errores por agregado se definen en `Domain/Aggregates/*/Errors/`.
- `InfrastructureExceptionBase` conserva un `ApplicationError` y un `HttpStatusCode` opcional (`Infrastructure/Exceptions/InfrastructureExceptionBase.cs`).
- `AuthenticatedUserBehavior` lanza error de aplicación `AUTH_REQUIRED` si falta autenticación y `FORBIDDEN` si falta `UserIdLocal`; solicitudes `IAllowAnonymousRequest` omiten esas comprobaciones (`Application/Common/Behaviors/AuthenticatedUserBehavior.cs`).

## Excepciones no controladas por handlers

`Api/Middleware/ExceptionHandlingMiddleware.cs` convierte las excepciones en `ResponseDto` con `TraceIdentifier` como correlación:

| Tipo capturado | HTTP | Cuerpo |
|---|---:|---|
| `Application.Exceptions.ValidationException` o FluentValidation | 400 | errores por campo y código de validación |
| `ApplicationExceptionBase` | 400 | código y descripción del error |
| `DomainExceptionBase` | 409 | código y descripción del error |
| `InfrastructureExceptionBase` | `HttpStatusCode` o 500 | código y descripción del error |
| `Exception` restante | 500 | `INTERNAL_SERVER_ERROR`; registra excepción |

## Respuestas devueltas por acciones

`BaseApiController.Send` es una ruta distinta: respuesta exitosa -> 200; códigos semánticos `NotFound`, `Unauthorized`, `Forbidden`, `InternalError` -> 404, 401, 403, 500; otros fallos -> 400. Algunas acciones devuelven estados explícitos. `ResponseDto<T>` tiene `Success`, `Code`, `Message`, `Data`, `Errors` y `CorrelationId`; `ResponseCodes` y `AppMessages` contienen códigos y mensajes HTTP de API separados de `ApplicationErrors`.

## Referencias

- Middleware: `src/COFIDE.WFF.Api/Middleware/ExceptionHandlingMiddleware.cs`.
- Respuesta y códigos: `src/COFIDE.WFF.Api/Controllers/DTOs/ResponseDto.cs`, `src/COFIDE.WFF.Api/Constants/ResponseCodes.cs`.
- Mapeo de `BaseApiController.Send`: `src/COFIDE.WFF.Api/Controllers/Common/BaseApiController.cs`.
- Excepciones: `src/COFIDE.WFF.Application/Exceptions/`, `src/COFIDE.WFF.Domain/Exceptions/`, `src/COFIDE.WFF.Infrastructure/Exceptions/`.
