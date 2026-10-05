# COFIDE.WFF.Infrastructure.Tests

Pruebas de los adaptadores de Infraestructura que se pueden ejecutar sin base de datos ni servicios externos.

## Estructura

```
Tests/        Pruebas, organizadas igual que src/COFIDE.WFF.Infrastructure
  Eventos/                Publicación post-commit inmediata y reintentos en segundo plano (B1)
  Adapters/FileStorage/   Almacenamiento, compensación transaccional, ZIP
Support/      Ayudantes compartidos (carpeta temporal, construcción de adaptadores, lectura de recursos)
Resources/    Datos de prueba en JSON (copiados al output por el csproj)
  (vacío por ahora)
```

## Reglas
- Una clase de prueba por clase de producción; mismo árbol de carpetas que `src/`.
- Los datos de casos van en `Resources/` (como en Domain.Tests y Application.Tests), no en el código.
- Nada en `Support/` contiene asserts: solo prepara el escenario.
- Las pruebas de archivos usan `DirectorioTemporal` (una carpeta por prueba, se borra al terminar).

## Fuera de alcance
Repositorios y consultas contra Oracle: se prueban en `COFIDE.WFF.Api.IntegrationTests` (Tests/Persistencia).
