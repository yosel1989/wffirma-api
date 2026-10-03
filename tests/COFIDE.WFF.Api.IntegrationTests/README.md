# COFIDE.WFF.Api.IntegrationTests

Pruebas contra la **base de datos de pruebas** (Oracle) y, opcionalmente, contra **servicios externos reales** (SUC).

## Estructura
```
Tests/
  Persistencia/   Repositorios y modelo EF contra Oracle (sin pasar por HTTP)
  Endpoints/      Punta a punta: HTTP → controlador → MediatR → handler → Oracle
  ApisExternas/   Pruebas de humo contra el SUC real (solo lectura, deshabilitadas por defecto)
Support/          WorkflowApiFactory, autenticación de prueba, dobles, configuración, atributos de omisión
Resources/        Datos de casos (JSON), por ejemplo los endpoints de consulta a verificar
  Configuracion/  conexiones.example.json (plantilla) y conexiones.json (local, no se versiona)
appsettings.IntegrationTests.json   Configuración NO secreta
```

## Configuración
| Qué | Dónde | ¿Se versiona? |
|---|---|---|
| Valores no sensibles (usuario de prueba, reintentos deshabilitados, flags) | `appsettings.IntegrationTests.json` | Sí |
| Plantilla de conexiones y credenciales | `Resources/Configuracion/conexiones.example.json` | Sí (sin valores) |
| Conexiones y credenciales de QA | `Resources/Configuracion/conexiones.json` | **No** (.gitignore) |
| Conexiones y credenciales en el pipeline | Variables de entorno `WFF_IT_...` marcadas como secretas | No |

Orden de prioridad: appsettings.IntegrationTests.json → conexiones.json → variables de entorno (la última gana).

### Local
1. Copie `Resources/Configuracion/conexiones.example.json` como `conexiones.json` en la misma carpeta.
2. Complete la cadena de conexión de la base de **pruebas** y, si va a ejecutar `Tests/ApisExternas`,
   las URLs/códigos de QA y `IntegrationTests:ApisExternasHabilitadas = true`.

### Pipeline
```
WFF_IT_ConnectionStrings__OracleConnection = <secreto>
WFF_IT_IntegrationTests__ApisExternasHabilitadas = false
```

Si falta la cadena de conexión, las pruebas se **omiten** (aparecen como *Skipped*), no fallan.

> **Temporal.** Cuando se adopte el gestor de secretos, solo cambia `Support/ConfiguracionPruebas.cs`
> (reemplazar la lectura de `conexiones.json`). Las pruebas no cambian.

## Ejecutar
```bash
dotnet test tests/COFIDE.WFF.Api.IntegrationTests                                   # todo lo configurado
dotnet test tests/COFIDE.WFF.Api.IntegrationTests --filter "Categoria=Integracion"  # solo base de datos
dotnet test --filter "Categoria!=Integracion&Categoria!=ApisExternas"               # solo pruebas rápidas de la solución
```

## Garantías
- **Nunca** se usa la base ni las APIs de producción: la cadena de conexión viene de la configuración de pruebas,
  no del SUC.
- Las pruebas de Persistencia trabajan dentro de una transacción que se **revierte**: no dejan datos.
- Correos y notificaciones en tiempo real están reemplazados por dobles (`Support/DoblesDePrueba.cs`).
- El usuario de prueba de `appsettings.IntegrationTests.json` debe existir en la base de pruebas.

## Requisitos de la base de pruebas
- Mismo esquema que producción (`US_WFFIRMAS`).
- Catálogos básicos cargados (áreas, tipos de solicitud, estados) para que los endpoints de consulta devuelvan datos.
