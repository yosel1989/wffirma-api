# /DevOps — Pipeline de api-wf-firmas

Backend del Workflow de Firmas. .NET 8, arquitectura hexagonal.

| Pipeline | Archivo | Alcance |
|---|---|---|
| `CICD-api-wf-firmas` | `/DevOps/ci-cd.yml` | Solo `COFIDE.WFF.Api` |

`COFIDE.WFF.Worker` queda fuera: es un proceso aparte (servicio de Windows o
tarea programada, no IIS) con su propio `appsettings.json`. Automatizarlo
requiere un plano de despliegue distinto al de IIS.

## ⚠ Bloqueante antes de registrar el pipeline

`src/COFIDE.WFF.Api/appsettings.json` versiona la contraseña de Oracle en
texto plano:

```
"OracleConnection": "User Id=US_WFFIRMAS;Password=...;Data Source=..."
```

El README del proyecto dice que la cadena se obtiene del SUC al arrancar
(`ExternalServices:SucWCF`), así que ese valor no debería estar en el archivo.
Con el pipeline en marcha pasa a vivir dentro de artefactos retenidos y
descargables.

Tres acciones, en este orden:

1. Vaciar el valor en `appsettings.json` (dejar `""`, como ya hace el Worker).
2. **Rotar la contraseña**: está en el historial de Git y seguirá ahí aunque
   se borre del archivo.
3. Verificar que el arranque contra el SUC funcione sin ella.

El punto 2 no es opcional. Cualquiera con acceso de lectura al repositorio
tiene hoy esa credencial.

## Ambientes

### DEV — verificado 20/10/2026 contra el IIS de SRVWEBBEDEV

| Atributo | Valor |
|---|---|
| Servidor | `SRVWEBBEDEV` |
| Sitio IIS | `WEB-BEDEV` (compartido) |
| **Aplicación virtual** | `/API-WFF` |
| Ruta física | `C:\inetpub\wwwroot\WEB-BEDEV\API-WFF` |
| Application Pool | `appPoolApiWFF`, identidad **`COFIDE\USRADMWFFDEV`** |
| Binding | `https` · `web-bedev.cofide.com.pe:443` con SNI · `http` en 89 |
| Certificado | `*.cofide.com.pe` |

⚠ **`API-WFF` no es un sitio, es una aplicación dentro de `WEB-BEDEV`**, que
hospeda además `API-SNT`, `API-SPB`, `API-UNICAS`, `API-SGP` y otras. El
manifiesto usa `aplicacionVirtual`, así que el pipeline no toca el sitio ni
sus bindings: son compartidos con el resto de las APIs. Si se desplegara como
sitio, se alterarían aplicaciones de otros equipos.

⚠ **El app pool corre con una identidad de dominio, no con
ApplicationPoolIdentity.** Esa cuenta es la que tiene acceso a Oracle, a
`C:\APP_DATADEV\WFF` y a las rutas de log. El pipeline no puede recrearla sin
almacenar su contraseña, y si intentara "asegurar" el app pool lo dejaría con
la identidad por defecto: la API arrancaría sin permisos.

Por eso el ambiente declara `gestionarIIS: 'no'`: el pipeline solo despliega
contenido. El sitio, la aplicación y el app pool se administran a mano, como
hasta ahora. Si en el futuro se automatizan, la identidad del app pool debe
resolverse antes.

Las carpetas de idioma (`cs`, `de`, `es`, `fr`, `ja`, `zh-Hans`...) **sí** las
produce el build: son los satellite assemblies de .NET y deben reemplazarse en
cada despliegue. Solo se preservan las carpetas `BK_*` de respaldos manuales.

### QA y PROD — pendientes de inventario

Hace falta:

| Dato | Por qué |
|---|---|
| Servidor y nombre del agente | `demands: Agent.Name -equals ...` |
| Sitio IIS contenedor y ruta física | Confirmar si también es aplicación virtual |
| ASP.NET Core Hosting Bundle 8.x instalado | Sin él IIS no levanta una app .NET 8 |
| Carpetas que no produce el build | Reglas `-skip` |

En DEV falta confirmar un dato: **el nombre del agente de Azure DevOps** en
`SRVWEBBEDEV`. El manifiesto asume que coincide con el nombre del servidor,
como ocurre en el frontend, pero conviene verificarlo en Agent pools.

Según `appsettings.json`, los documentos van a `C:\APP_DATADEV\WFF` y los logs
a `C:\APP_LOGS\DEV\WFF`, ambos fuera del directorio del sitio. Conviene
confirmarlo: si algo vive dentro, `RemoveAdditionalFilesFlag` lo borra.

## Variable Groups

Un grupo por ambiente, con notación de punto para claves anidadas. **La cadena
de conexión y la clave JWT no van aquí**: la Api las obtiene del SUC.

| Variable | DEV |
|---|---|
| `FrontEnd.BaseUrl` | `https://workflowfirmas-dev.cofide.com.pe/` |
| `BackEnd.BaseUrl` | `https://web-bedev.cofide.com.pe/` |
| `FileServer.BaseUrl` | `C:\APP_DATADEV\WFF` |
| `ExternalServices.SucApi.BaseUrl` | `https://apigwdev.cofide.com.pe/API-GATEWAY/api-suc_v2/` |
| `ExternalServices.CorreoApi.BaseUrl` | `https://api-dev.cofide.com.pe/API-CORREO/` |
| `ExternalServices.FirmaApi.BaseUrl` | `https://apigwdev.cofide.com.pe/API-GATEWAY/api-fird/` |
| `ExternalServices.FirmaApi.EndpointCallBack` | `https://apigwdev.cofide.com.pe/API-GATEWAY/API-WFF/` |
| `ExternalServices.HubSocketApi.BaseUrl` | `https://skhubdev.cofide.com.pe/` |
| `ExternalServices.SucWCF.BaseUrl` | `http://srvwebdev/SUC_SERVICIO/ConfiguracionSUC.svc` |
| `SerilogOptions.InfoLogPath` | `C:\APP_LOGS\DEV\WFF\workflow_info-.log` |
| `SerilogOptions.ErrorLogPath` | `C:\APP_LOGS\DEV\WFF\workflow_error-.log` |

`CorsOptions.Origins` es un arreglo y la sustitución JSON del task de IIS no
maneja arreglos de forma fiable. Déjalo fijo en `appsettings.json` con los
orígenes de todos los ambientes, o resuélvelo con una transformación aparte.

`JwtSettings.Issuer` y `Audience` apuntan hoy a `http://localhost:4200` incluso
en el archivo de DEV. Confirmar con el equipo si es correcto o quedó de
desarrollo local.

## Verificación posterior al despliegue

El README del proyecto define cuatro comprobaciones. El smoke test del pipeline
cubre la primera; las otras tres conviene correrlas a mano la primera vez:

```bash
curl -I  https://<host>/API-WFF/swagger/index.html          # 200
curl -i  https://<host>/API-WFF/swagger/v1/swagger.json     # 200
curl -i  https://<host>/API-WFF/api/v1/maestros/areas       # 401/403, no 500
curl -i  https://<host>/API-WFF/api/v1/public/validacion-despliegue
#  -> 4xx con APP_SOL_010. Un 500 indica problema con Oracle o con el SUC.
```

La cuarta es la más valiosa: confirma que la Api llegó a la base de datos y
obtuvo su configuración del SUC. Vale la pena incorporarla al smoke test del
pipeline una vez que el despliegue sea estable.

## Pendientes

- Automatizar `COFIDE.WFF.Worker`
- Swagger está habilitado en todos los ambientes (lo dice el README del
  proyecto). Decidir si en PROD debe condicionarse en `Program.cs`
- Definir el umbral de cobertura y reactivar `ejecutarSonar`
