using COFIDE.WFF.Domain.Common.Errors;
namespace COFIDE.WFF.Domain.Aggregates.SolicitudAggregate.Errors;
public static class SolicitudErrors
{
    public static readonly DomainError AsuntoRequired =
        new("SOLICITUD_ASUNTO_REQUIRED", "El asunto es obligatorio.");

    public static readonly DomainError MensajeRequired =
        new("SOLICITUD_MENSAJE_REQUIRED", "El mensaje es obligatorio.");

    public static readonly DomainError DocumentoRequired =
        new("SOLICITUD_DOCUMENTO_REQUIRED", "Debe adjuntar al menos un documento.");

    public static readonly DomainError SustentoRequired =
        new("SOLICITUD_SUSTENTO_REQUIRED", "Debe adjuntar al menos un sustento.");

    public static readonly DomainError CorreoRequired =
        new("SOLICITUD_CORREO_REQUIRED", "Debe registrar al menos un correo.");

    // General

    public static readonly DomainError FinalizarSoloBorrador = new("DOM_SOL_001", "Solo se puede finalizar una solicitud en estado Borrador.");
    public static readonly DomainError EditarSoloBorrador = new("DOM_SOL_002", "Solo se puede editar una solicitud en estado Borrador.");
    public static readonly DomainError FechaLimiteFuturo = new("DOM_SOL_003", "La fecha límite debe ser futura.");
    public static readonly DomainError RechazarSoloEnCurso = new("DOM_SOL_004", "Solo se puede rechazar una solicitud en estado En Curso.");
    public static readonly DomainError SolicitudRechazada = new("DOM_SOL_005", "La solicitud se encuentra rechazada");
    public static readonly DomainError FlujoRequired = new("DOM_SOL_006", "La solicitud no tiene una configuración de flujo.");
    public static readonly DomainError SolicitudInvalida = new("DOM_SOL_007", "La solicitud es inválida.");
    public static readonly DomainError UsuarioInvalido = new("DOM_SOL_008", "Usuario es inválida.");
    public static readonly DomainError PlantillaAsignada = new("DOM_SOL_009", "La solicitud ya tiene una plantilla asignada.");
    // Solicitud Documento
    public static readonly DomainError CantidadLimiteDocumentos = new("DOM_SOL_010", "Sólo se permite {0} documentos.");
    // Solicitud Sustento
    public static readonly DomainError CantidadLimiteSustentos = new("DOM_SOL_011", "Sólo se permite {0} sustentos.");
    // Aprobaciones
    public static readonly DomainError AprobSoloAprobEnCurso = new("DOM_SOL_012", "Solo se puede aprobar la solicitud en curso.");
    public static readonly DomainError AprobDocumentosNoFirmados = new("DOM_SOL_013", "Existen documentos no firmados.");
    public static readonly DomainError AprobFechaLimiteUrgente = new("DOM_SOL_014", "Debe existir una fecha límite cuando la solicitud es urgente.");
    public static readonly DomainError NoPuedeAprobarSolicitud = new("DOM_SOL_015", "No tiene permisos para aprobar esta solicitud.");
    public static readonly DomainError NoPuedeAprobarEstaEtapaSolicitud = new("DOM_SOL_016", "No puedes aprobar en esta etapa aún");
    // Rechazo
    public static readonly DomainError MotivoRechazoRequired = new("DOM_SOL_017", "El motivo de rechazo es obligatorio.");
    public static readonly DomainError MotivoRechazoLength = new("DOM_SOL_018", "El Motivo de Rechazo no debe exceder de los {0} caracteres.");
    public static readonly DomainError NoPuedeRechazarSolicitud = new("DOM_SOL_019", "No tiene permisos para rechazar esta solicitud.");
    // Solicitud Evento
    public static readonly DomainError TipoEventoInvalido = new("DOM_SOL_020", "Tipo de evento inválido.");
    // Solicitud Comentario
    public static readonly DomainError ComentarioRequired = new("DOM_SOL_021", "El comentario es requerido.");
    // Firmante Flujo
    public static readonly DomainError FirmanteAprobado = new("DOM_SOL_022", "Ya se encuentra aprobado.");
    public static readonly DomainError FirmanteModificacionNoValida = new("DOM_SOL_023", "No puedes modificar un firmante aprobado.");
    public static readonly DomainError FirmanteEliminacionNoValida = new("DOM_SOL_024", "No se puede eliminar un firmante ya aprobado.");
    // Solicitud Código Referencia
    public static readonly DomainError CodReferenciaRequired = new("DOM_SOL_025", "Código de referencia es requerido.");
    public static readonly DomainError CodReferenciaLength = new("DOM_SOL_026", "El Código de Referencia no debe exceder de los {0} caracteres.");
    // Detalle Firma
    public static readonly DomainError DocumentoFirmado = new("DOM_SOL_027", "El documento ya ha sido firmado.");
    public static readonly DomainError DocumentoSinHash = new("DOM_SOL_028", "El documento no pertenece a la solicitud.");
    public static readonly DomainError CrearEtapaPrevia1 = new("DOM_SOL_031", "No se puede crear una etapa previa, existe un documento firmado o solicitud aprobada.");
    public static readonly DomainError CrearEtapaPrevia = new("DOM_SOL_032", "No puedes insertar una etapa antes o en una etapa que ya tiene actividad.");
    // Anular
    public static readonly DomainError SolicitudYaAnulada = new("DOM_SOL_029", "La solicitud ya fue anulada.");
    public static readonly DomainError SolicitudAprobadaNoSePuedeAnular = new("DOM_SOL_030", "la solicitud ya fue aprobada.");

    public static readonly DomainError PermisoEditarSolicitud = new("DOM_SOL_033", "No tiene permisos para editar esta solicitud.");
    public static readonly DomainError PermisoVisualizarSolicitud = new("DOM_SOL_034", "No tiene permisos para visualizar esta solicitud.");
    public static readonly DomainError PermisoCoordinadorSolicitud = new("DOM_SOL_035", "No tiene permisos para editar esta solicitud o pertenece a otra área.");
    public static readonly DomainError PermisSolicitud = new("DOM_SOL_036", "No tiene permisos para esta solicitud.");
    public static readonly DomainError PermisAprobarRechazarSolicitud = new("DOM_SOL_037", "Sólo se puede aprobar/rechazar la solicitud en estado En Curso.");

    public static readonly DomainError DocumentoTagInvalid = new("DOM_SOL_038", "Documento Tag inválido.");
    public static readonly DomainError SolicitudDocumentoInvalid = new("DOM_SOL_039", "Solicitud Documento Tag inválido.");

    public static readonly DomainError FirmanteNotFound = new("DOM_SOL_040", "No se encontraron firmantes.");

    public static readonly DomainError RestaurarEstadoNoPermitido = new("DOM_SOL_041", "Solo se puede restaurar una solicitud en curso, aprobada o rechazada.");
    public static readonly DomainError PuntoRestauracionNoEncontrado = new("DOM_SOL_042", "El punto de restauración no pertenece al flujo de la solicitud.");
    public static readonly DomainError PuntoRestauracionSinFecha = new("DOM_SOL_043", "El punto de restauración no tiene fecha de revisión.");

    public static readonly DomainError CambioEstadoNoPermitido = new("DOM_SOL_044", "El cambio de estado solicitado no está permitido para el estado actual de la solicitud.");
    public static readonly DomainError CambioEstadoMotivoRequired = new("DOM_SOL_045", "El motivo del cambio de estado es obligatorio.");
    public static readonly DomainError CambioEstadoMotivoLength = new("DOM_SOL_046", "El motivo del cambio de estado no debe exceder los {0} caracteres.");
    public static readonly DomainError ReabrirSinEtapaNueva = new("DOM_SOL_047", "Debe agregar al menos una etapa al final del flujo para devolver la solicitud a En curso.");
    public static readonly DomainError CambioEstadoSinEtapaActual = new("DOM_SOL_048", "La solicitud no tiene una etapa pendiente de aprobación.");
    public static readonly DomainError RechazoSinFirmante = new("DOM_SOL_049", "No se encontró el usuario que rechazó la solicitud.");
}
