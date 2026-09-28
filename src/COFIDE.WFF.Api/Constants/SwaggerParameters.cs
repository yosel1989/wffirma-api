namespace COFIDE.WFF.Api.Constants;

internal static class SwaggerParameters
{
    public const string IdTipoSolicitud = "<li><span><b>IdTipoSolicitud:</b> Tipo principal de solicitud a registrar.    -   [Obligatorio:string]</span></li>";
    public const string IdSubTipoSolicitud = "<li><span><b>IdSubTipoSolicitud:</b> Categoría específica asociada al tipo de solicitud.     -   [Obligatorio:string]</span></li>";
    public const string IdtipoOperacion = "<li><span><b>IdtipoOperacion:</b> Tipo de operación o acción que se realizará sobre la solicitud.      -   [Obligatorio:File]</span></li>";
    public const string Asunto = "<li><span><b>Asunto:</b> Título o resumen breve de la solicitud..      -   [Obligatorio:string]</span></li>";
    public const string Mensaje = "<li><span><b>Mensaje:</b> Descripción detallada del requerimiento o motivo.      -   [Opcional:integer]</span></li>";
    public const string EsUrgente = "<li><span><b>EsUrgente:</b> Indica si la solicitud requiere atención prioritaria.      -   [Opcional:boolean]</span></li>";
    public const string InCodigo = "<li><span><b>InCodigo:</b> Define si la solicitud incluye o requiere el código de solicitud.     -   [Opcional:boolean]</span></li>";
    public const string FechaLimite = "<li><span><b>FechaLimite:</b> Fecha y hora máxima para atender o procesar la solicitud.      -   [Opcional:Datetime]</span></li>";
    public const string PesoTotalDocu = "<li><span><b>PesoTotalDocu:</b> Tamaño total acumulado de los documentos adjuntos.      -   [Opcional:integer]</span></li>";
    public const string Documentos = "<li><span><b>Documentos:</b> Lista de documentos principales adjuntados a la solicitud.      -   [Opcional:Array[File]]</span></li>";
    public const string Sustentos = "<li><span><b>Sustentos:</b> Lista de archivos o evidencias de soporte adjuntadas.      -   [Opcional:Array[File]]</span></li>";
    public const string CodigosReferencia = "<li><span><b>CodigosReferencia:</b> Códigos de referencias asociadas a la solicitud.      -   [Opcional:string]</span></li>";
    public const string IdPlantilla = "<li><span><b>IdPlantilla:</b> Identificador de la plantilla utilizada para la solicitud.      -   [Opcional:integer]</span></li>";
    public const string Configuracion = "<li><span><b>Configuracion:</b> Configuración del flujo.      -   [Opcional:JSON]</span></li>";
    public const string Correos = "<li><span><b>Correos:</b> Lista de correos electrónicos asociados para notificación o seguimiento.      -   [Opcional:string]</span></li>";
    public const string Tags = "<li><span><b>Tags:</b> Lista de tags configurados para cada documento.      -   [Opcional:JSON]</span></li>";

    public const string IdsDocumentosConservar = "<li><span><b>IdsDocumentosConservar:</b> Lista de documentos que serán conservados y no eliminados.      -   [Opcional:Array[string]]</span></li>";
    public const string IdsSustentosConservar = "<li><span><b>IdsSustentosConservar:</b> Lista de sustentos que serán conservados y no eliminados.      -   [Opcional:Array[string]]</span></li>";
}
