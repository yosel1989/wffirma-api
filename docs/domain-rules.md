# Reglas de dominio

## Visibilidad y permisos de solicitud

`Domain/DomainServices/ReglaConsultaSolicitud.cs` genera una expresión usada para filtrar consultas y validar una solicitud individual.

- Administrador: predicado permite cualquier solicitud.
- Coordinador: requiere `idArea`; permite el área persistida igual al área del usuario, o el área/área padre dentro de accesos especiales.
- Solicitante-aprobador: requiere `idUsuario`; permite solicitudes propias, o el área/área padre dentro de accesos especiales.
- Contexto faltante, rol desconocido o solicitud nula deniega acceso. Acceso especial habilita consulta, no escritura; el agregado lanza `PermisoEscrituraAccesoEspecial` al validar escritura.
- `ReglaAccesoEspecial.EvaluarRolAccesoEspecial` devuelve true únicamente para rol Coordinador.

## Solicitudes y aprobación

En `Domain/Aggregates/SolicitudAggregate/Solicitud.cs`:

- `Finalizar` solo acepta estado Borrador. `EditarSolicitudBorradorYEnviar` acepta Borrador, Rechazado o EnCurso.
- Se rechazan más de 30 documentos o más de 100 sustentos.
- Para aprobar, la solicitud debe estar EnCurso, ningún detalle de firma activo de sus documentos activos puede estar sin firmar y, si es urgente, debe tener fecha límite. La aprobación del firmante requiere que sea su etapa y que etapas anteriores estén aprobadas; los documentos vinculados a aprobadores anteriores deben estar firmados.
- Rechazo solo en EnCurso; requiere motivo de hasta 350 caracteres.
- `PuedeCambiarEstado` permite EnCurso -> Rechazado/Borrador y Aprobado/Rechazado -> EnCurso. `ValidarCambioEstado` además exige motivo no vacío y dentro de la longitud máxima definida por el agregado.
- Reabrir una solicitud aprobada requiere que quede una etapa pendiente; reabrir una rechazada requiere identificar al firmante que la rechazó.
- `FechaLimite.Crear` rechaza fechas anteriores a `HoraSistema.Ahora` (`src/COFIDE.WFF.Domain/Aggregates/SolicitudAggregate/Vo/FechaLimite.cs`).

## Plantillas y flujo de firmas

- Una plantilla pública debe ser reutilizable. El flujo debe tener al menos una etapa activa; los órdenes comienzan en 1 y son consecutivos; el total de firmantes de etapas activas no excede 20 (`Plantilla.ValidarFlujoCompleto`).
- La etapa secuencial requiere exactamente un firmante; la paralela, al menos dos (`src/COFIDE.WFF.Domain/Aggregates/PlantillaAggregate/Vo/TipoEtapa.cs`).
- En `PermitirActualizarPlantilla`, un coordinador debe pertenecer a la gerencia de una plantilla pública; para una plantilla privada, un solicitante debe ser propietario. `ValidarPermisoPlantilla` comprueba gerencia del coordinador y propietario del solicitante en plantilla pública.
- Para etapa ya aprobada no se puede eliminar ni cambiar orden, tipo o firmantes (`WorkflowDomainService.ValidacionFirmanteNoMaestroFlujo`).
- Al crear/reutilizar solicitud, `ReglaPlantillaSolicitud` decide si usar flujo sin plantilla, crear desde configuración/maestra, o actualizar plantilla de origen/seleccionada. La maestra se copia y no se modifica.

## Referencias

- Solicitud: `src/COFIDE.WFF.Domain/Aggregates/SolicitudAggregate/Solicitud.cs`.
- Errores de solicitud: `src/COFIDE.WFF.Domain/Aggregates/SolicitudAggregate/Errors/SolicitudErrors.cs`.
- Visibilidad: `src/COFIDE.WFF.Domain/DomainServices/ReglaConsultaSolicitud.cs`.
- Plantillas: `src/COFIDE.WFF.Domain/Aggregates/PlantillaAggregate/Plantilla.cs`, `src/COFIDE.WFF.Domain/Aggregates/PlantillaAggregate/Vo/TipoEtapa.cs`.
- Validación del flujo: `src/COFIDE.WFF.Domain/DomainServices/WorkflowDomainService.cs`.
- Selección de plantilla: `src/COFIDE.WFF.Domain/DomainServices/ReglaPlantillaSolicitud.cs`.
- Pruebas de visibilidad: `tests/COFIDE.WFF.Domain.Tests/Tests/Solicitudes/ReglaConsultaSolicitudTest.cs`.
