# COFIDE.WFF.Architecture.Tests

Pruebas automáticas de arquitectura: verifican que la solución cumpla las reglas de la
arquitectura hexagonal, DDD y CQRS que **sí pueden comprobarse con código**. Cada prueba
está vinculada al hallazgo correspondiente del informe de evaluación (`Trait("Hallazgo", ...)`).

## Ejecutar

```bash
dotnet test tests/COFIDE.WFF.Architecture.Tests

# Solo una categoría o un hallazgo
dotnet test tests/COFIDE.WFF.Architecture.Tests --filter "Categoria=Hexagonal"
dotnet test tests/COFIDE.WFF.Architecture.Tests --filter "Hallazgo=H1"
```

Categorías: `Hexagonal`, `DDD`, `CQRS`, `UI`, `Transversal`, `BuenasPracticas`, `Nomenclatura`.

Las pruebas con `Trait("Tipo", "Senal")` no demuestran un error: señalan código que **probablemente**
contiene lógica de negocio fuera del dominio y debe confirmarse en revisión. Para ejecutarlas o excluirlas:

```bash
dotnet test tests/COFIDE.WFF.Architecture.Tests --filter "Tipo=Senal"
dotnet test tests/COFIDE.WFF.Architecture.Tests --filter "Tipo!=Senal"
```

## Cómo verifica

| Técnica | Qué revisa | Clases de soporte |
|---|---|---|
| Lectura de `.csproj` | Referencias entre proyectos, paquetes NuGet y versiones | `ProyectoMsBuild` |
| IL con **NetArchTest** | Dependencias reales de cada tipo (incluye cuerpos de métodos) | `Dependencias` |
| Reflexión | Setters, firmas, parámetros de acciones, puertos implementados | `ReflexionExtensions`, `Capas` |
| Código fuente (sin comentarios) | HTML, URLs, literales, `DateTime.Now`, `dynamic`, appsettings | `RutasSolucion`, `Codigo` |

El Worker no se referencia (es un ejecutable independiente); se valida por su `.csproj` y su código fuente.

## Línea base ("trinquete")

`Support/LineaBase.cs` está **vacía a propósito**: la primera ejecución muestra el estado real
del código. Cada prueba que falla indica la regla, la recomendación y la lista de violaciones.

Para incorporar las pruebas al pipeline sin bloquearlo mientras se corrige la deuda:

1. Ejecute las pruebas.
2. Copie en `LineaBase.cs` el bloque que imprime cada prueba fallida
   (sección *"Si la deuda es conocida..."*).
3. Desde ese momento una prueba falla solo si:
   - aparece una violación **nueva** (deuda que no existía), o
   - una violación registrada **ya se corrigió** (hay que quitarla de la línea base para que no reaparezca).

Así la deuda solo puede disminuir.

## Reglas incluidas

| Archivo | Regla | Hallazgo |
|---|---|---|
| MatrizDeReferenciasTests | Lista blanca de referencias por proyecto (cualquier referencia no prevista falla) | Base |
| | Sin referencias circulares; SharedKernel por debajo de Domain | Base |
| ReferenciasEntreProyectosTests | Domain sin referencias ni paquetes | Base |
| | Application solo referencia Domain / SharedKernel | Base |
| | Application sin paquetes de tecnología (ASP.NET, EF, HTTP…) | H1 |
| | Infrastructure no referencia Api/Worker; hosts independientes | Base |
| | SharedKernel sin dependencias; CrossCutting fuera del núcleo | C1-C2 |
| | Versiones de paquetes consistentes, sin comodines y alineadas al TargetFramework | B9 |
| DependenciasEntreCapasTests | Domain no depende de otras capas ni de tecnología | Base |
| | Application no depende de adaptadores | Base |
| | Application sin ASP.NET/EF/HTTP/PDF | H1 |
| | Application sin System.IO.Compression | H2 |
| | Controladores sin Infrastructure; Middleware sin excepciones de Infrastructure | Base / C1 |
| PuertosYAdaptadoresTests | Carpeta Ports solo con interfaces; Application no implementa sus Ports.Out | Base |
| | Cada Ports.Out tiene adaptador; repositorios implementan puertos | Base |
| | Interfaces implementadas por adaptadores están en Ports.Out; sin namespace Application.Ports | H4 |
| | Sin Ports.In / UseCase sobre MediatR | A1 |
| ModeloDeDominioTests | Entidades sin setters públicos | D6 |
| | Value objects y eventos inmutables; entidades sin constructor público vacío | Base |
| | Sin DTOs ni paginación en Domain | D5 |
| | Sin banderas de persistencia (EsNuevo) ni `Func<long>` para Ids | D4 |
| ReglasDeNegocioFueraDelDominioTests | Roles como texto y números mágicos de estado en repositorios | D1 |
| | Códigos de catálogo como texto en Application | D8 |
| | `DateTime.Now` en Domain/Application | D7 |
| | Application lanzando `DomainException` | D3 / A2 |
| LogicaDeNegocioEnDominioTests | Raíces de agregado identificables y con comportamiento (no anémicas) | Base |
| | Infrastructure/Api no lanzan excepciones de dominio; servicios de dominio solo en Domain | Base |
| | Adaptadores sin catálogos de errores de negocio ni métodos Validar* que devuelven mensajes | D2 |
| | Application no instancia entidades con `new` | D6 / A2 |
| | *Señal:* servicios con lógica en Application | A2 |
| | *Señal:* handlers/servicios con demasiadas decisiones | A2 |
| | *Señal:* decisiones sobre el estado de entidades en Application (Tell, don't ask) | A2 |
| CapaAplicacionTests | Commands/Queries sin tipos web | H1 |
| | Sufijo Command/Query; Queries no son Commands | B7 / Base |
| | Handlers sin otros handlers, sin UseCase intermedio, sin clases concretas | Base / A1 |
| | Sin Requests HTTP en Application | H6 |
| CapaApiTests | Acciones no reciben Commands/Queries ni modelos de Application | H6 |
| | Acciones async con CancellationToken | B2 |
| | Controladores sin repositorios ni UseCases | Base |
| | Requests bajo una sola raíz de namespace | N6 |
| AcoplamientoInterfazUsuarioTests | Sin HTML/estilos en el núcleo | U1-U4 |
| | Sin URLs del front/back armadas en el núcleo | U1-H7 |
| | Sin nombres de controles visuales (Combo, Grid…) | U2 |
| | Sin decisiones de navegación (Redirigir…) | U3 |
| CodigoTransversalTests | Un solo tipo Error y una sola excepción base | C1 |
| | Logging configurado en un solo lugar; correlación fuera de Application | C2 |
| | Métodos privados copiados entre clases | C3 |
| BuenasPracticasTests | Sin `dynamic`, sin `Console.Write`, transacción sin depender del nombre | B13 / B6 / B7 |
| | Sin `catch (Exception)` en Application; sin sistema de archivos en Application | B5 / H2 |
| | HttpClient con resiliencia; FallbackPolicy; appsettings base limpio | B8 / B12 / B11 |
| NomenclaturaTests | Archivos sin espacios; archivo = tipo | N1 / Base |
| | Sufijo Async solo en métodos awaitables; métodos sin guion bajo | N7 |
| | Errores ortográficos conocidos | N2 |
| | Nombres de tipos duplicados entre proyectos/namespaces | N5 / C2 |

## Resultado esperado hoy

- **Deberían pasar** las reglas marcadas como `Base`: confirman lo que el proyecto ya hace bien
  (Domain sin dependencias, dirección de referencias, controladores delgados). Si alguna falla,
  es un hallazgo adicional no detectado en la revisión manual.
- **Deberían fallar** las reglas asociadas a hallazgos del informe (H, D, A, U, C, B, N) hasta
  que se corrijan. Esa falla es el resultado buscado: lista exacta de clases por corregir.

## Lo que NO se puede verificar automáticamente (juicio experto)

Estas reglas quedan para la revisión de código (PR) y el informe:

- Si un Application Service contiene **reglas de negocio** que pertenecen al dominio (A2): las pruebas
  de `LogicaDeNegocioEnDominioTests` y `ReglasDeNegocioFueraDelDominioTests` lo señalan, pero la
  confirmación es de revisión experta.
- Si la **política de secuencia de firmas** vive en el validador PDF de Infraestructura (D2).
- Si los **límites de un agregado** son correctos o el agregado es demasiado grande.
- Si un Handler es un simple **pass-through** sin responsabilidad propia.
- Si un nombre expresa bien el **lenguaje ubicuo** (mezcla español/inglés, abreviaturas) (N3, N4).
- Consistencia transaccional con archivos y **Outbox** (B1).
- Si una consulta de lectura está **duplicada** en otro repositorio (H3).
- Si la cobertura de **pruebas del dominio** es suficiente (B10).

## Mantenimiento

- Para agregar una regla, cree un `[Fact]` con `Trait("Categoria", ...)` y `Trait("Hallazgo", ...)`
  y use `ArchitectureAssert.SinViolaciones(regla, violaciones, recomendacion)`.
- El nombre de la regla (primer parámetro) es la clave de la línea base: no lo cambie si ya
  tiene entradas registradas.
- Si se crean los proyectos `COFIDE.WFF.SharedKernel` o `COFIDE.WFF.CrossCutting`, las reglas
  que los mencionan se activan automáticamente.
