namespace COFIDE.WFF.Api.Constants;

public static class DescriptionSwagger
{
    private const string ResumenHtml = "<p><b>Resumen</b></p>";
    private const string ParametrosHtml = "<p><b>Parámetros:</b></p>";
    private const string CodeStart = "<pre><code>";
    private const string CodeEnd = "</code></pre>";

    public struct SolicitudController
    {
        public struct Create
        {
            public const string Summary = "Creación de una solicitud como Borrador";

            public const string Description =
                    ResumenHtml +
                    "<ul><li>Creación de una solicitud como Borrador</li></ul>" +
                    ParametrosHtml +
                    "<ul>" +
                    SwaggerParameters.IdTipoSolicitud +
                    SwaggerParameters.IdSubTipoSolicitud +
                    SwaggerParameters.IdtipoOperacion +
                    SwaggerParameters.Asunto +
                    SwaggerParameters.Mensaje +
                    SwaggerParameters.EsUrgente +
                    SwaggerParameters.InCodigo +
                    SwaggerParameters.FechaLimite +
                    SwaggerParameters.PesoTotalDocu +
                    SwaggerParameters.Documentos +
                    SwaggerParameters.Sustentos +
                    SwaggerParameters.CodigosReferencia +
                    SwaggerParameters.IdPlantilla +
                    SwaggerParameters.Configuracion +
                    SwaggerParameters.Correos +
                    SwaggerParameters.Tags +

                    "<p><b>Ejemplo Configuración del flujo:</b></p>" +
                    SwaggerExamples.ConfiguracionCrear +
                    "<p><b>Ejemplo Tags:</b></p>" +
                    SwaggerExamples.TagsCrear +
                    "</ul>";
        }

        public struct CreateEnviar
        {
            public const string Summary = "Creación y envío de solicitud";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite crear una solicitud en estado Finalizado para iniciar el proceso de revisión y posterior aprobación o rechazo.</li></ul>";
        }

        public struct CrearSolicitudPorReuso
        {
            public const string Summary = "Creación de solicitud por reuso";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite crear una nueva solicitud reutilizando la información de una solicitud existente.</li></ul>";
        }

        public struct CrearSolicitudPorReusoYEnviar
        {
            public const string Summary = "Creación y envío de solicitud por reuso";

            public const string Description = ResumenHtml +
                "<ul><li>Permite crear una nueva solicitud reutilizando la información de una solicitud existente y marcarla como Finalizada para iniciar el proceso de revisión, aprobación o rechazo.</li></ul>";
        }

        public struct EditarSolicitud
        {
            public const string Summary = "Edición de solicitud";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite actualizar una solicitud existente, siempre que se encuentre en un estado editable y el usuario tenga los permisos correspondientes.</li></ul>" +
                ParametrosHtml +
                "<ul>" +
                SwaggerParameters.IdTipoSolicitud +
                SwaggerParameters.IdSubTipoSolicitud +
                SwaggerParameters.IdtipoOperacion +
                SwaggerParameters.Asunto +
                SwaggerParameters.Mensaje +
                SwaggerParameters.EsUrgente +
                SwaggerParameters.InCodigo +
                SwaggerParameters.FechaLimite +
                SwaggerParameters.PesoTotalDocu +
                SwaggerParameters.Documentos +
                SwaggerParameters.Sustentos +
                SwaggerParameters.IdsDocumentosConservar +
                SwaggerParameters.IdsSustentosConservar +
                SwaggerParameters.CodigosReferencia +
                SwaggerParameters.IdPlantilla +
                SwaggerParameters.Configuracion +
                SwaggerParameters.Correos +
                SwaggerParameters.Tags +

                   "<p><b>Ejemplo Configuración del flujo:</b></p>" +

                   CodeStart +
                   "{\n" +
                   "  \"visibilidad\": false,\n" +
                   "  \"esReutilizable\": false,\n" +
                   "  \"etapas\": [\n" +
                   "    {\n" +
                   "      \"id\": 300,\n" +
                   "      \"ordenEtapa\": 1,\n" +
                   "      \"nombreEtapa\": \"Etapa 1\",\n" +
                   "      \"idTipoEtapa\": 1,\n" +
                   "      \"idTipoAprobador\": 3,\n" +
                   "      \"firmantes\": [\n" +
                   "        {\n" +
                   "          \"id\": 325,\n" +
                   "          \"idUsuarioAsignado\": 66,\n" +
                   "          \"idUsuarioOrigen\": 66,\n" +
                   "          \"cargoUsuario\": \"ASISTENTE\",\n" +
                   "          \"areaUsuario\": \"DEPARTAMENTO DE OPERACIONES BACK OFFICE\",\n" +
                   "          \"gerenciaUsuario\": \"GERENCIA GENERAL\"\n" +
                   "        }\n" +
                   "      ]\n" +
                   "    },\n" +
                   "    {\n" +
                   "      \"id\": 0,\n" +
                   "      \"ordenEtapa\": 2,\n" +
                   "      \"nombreEtapa\": \"Etapa 2\",\n" +
                   "      \"idTipoEtapa\": 2,\n" +
                   "      \"idTipoAprobador\": 4,\n" +
                   "      \"firmantes\": [\n" +
                   "        {\n" +
                   "          \"idUsuarioAsignado\": 68,\n" +
                   "          \"idUsuarioOrigen\": 68,\n" +
                   "          \"cargoUsuario\": \"ASISTENTE\",\n" +
                   "          \"areaUsuario\": \"DEPARTAMENTO DE ADMINISTRACION DE FIDEICOMISOS\",\n" +
                   "          \"gerenciaUsuario\": \"GERENCIA GENERAL\"\n" +
                   "        },\n" +
                   "        {\n" +
                   "          \"idUsuarioAsignado\": 38,\n" +
                   "          \"idUsuarioOrigen\": 38,\n" +
                   "          \"cargoUsuario\": \"SUPERVISOR\",\n" +
                   "          \"areaUsuario\": \"DEPARTAMENTO DE ADMINISTRACION DE FIDEICOMISOS\",\n" +
                   "          \"gerenciaUsuario\": \"GERENCIA GENERAL\"\n" +
                   "        }\n" +
                   "      ]\n" +
                   "    }\n" +
                   "  ]\n" +
                   "}" +
                   CodeEnd +


                "<p><b>Ejemplo Tags:</b></p>" +
                SwaggerExamples.TagsEditar +
                "</ul>";
        }

        public struct EditarBorradorYFinalizar
        {
            public const string Summary = "Edición y finalización de borrador";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite actualizar una solicitud en estado Borrador y marcarla como Finalizada para iniciar el proceso de revisión, aprobación o rechazo.</li></ul>";
        }

        public struct AprobarSolicitud
        {
            public const string Summary = "Aprueba una solicitud en proceso de revisión";

            public const string Description = ResumenHtml +
                "<ul><li>Permite aprobar una solicitud, registrando la conformidad del usuario responsable y actualizando el estado correspondiente del flujo de revisión.</li></ul>";
        }

        public struct RechazarSolicitud
        {
            public const string Summary = "Rechaza una solicitud en proceso de revisión";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite rechazar una solicitud, registrando la observación o motivo correspondiente y actualizando el estado del flujo de revisión.</li></ul>";
        }

        public struct AnularSolicitud
        {
            public const string Summary = "Anula una solicitud registrada";

            public const string Description = ResumenHtml +
                "<ul><li>Permite anular una solicitud registrada, siempre que cumpla las condiciones de negocio definidas para su anulación.</li></ul>";
        }

        public struct ObtenerPuntosRestauracion
        {
            public const string Summary = "Obtiene los puntos de restauración de una solicitud";

            public const string Description = ResumenHtml +
                "<ul><li>Devuelve las intervenciones aprobadas (CH_REV_APROBADO = S) y rechazadas (CH_REV_APROBADO = N) del flujo de la solicitud, en el orden de la plantilla.</li></ul>";
        }

        public struct RestaurarSolicitud
        {
            public const string Summary = "Restaura el flujo de una solicitud a un punto";

            public const string Description = ResumenHtml +
                "<ul><li>Reinicia el punto elegido y las intervenciones posteriores. Si la solicitud está aprobada o en curso y el interviniente firmó y aprobó, elimina el archivo firmado, inactiva el detalle de firma y crea uno nuevo por documento. Si está rechazada, crea un nuevo detalle de firma por documento. En ambos casos deja CH_REV_APROBADO en N, la fecha de revisión en nulo e inactiva el historial. La solicitud queda en curso.</li></ul>";
        }
    }
    public struct FirmaController
    {
        public struct FirmaInternaPdf
        {
            public const string Summary = "Firma individual sólo para documentos PDF";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite iniciar el proceso de firma interna para un documento PDF asociado a una solicitud.</li></ul>";
        }

        public struct FirmaInternaZip
        {
            public const string Summary = "Firma masiva sólo para archivos ZIP";

            public const string Description = ResumenHtml +
                "<ul><li>Permite iniciar el proceso de firma interna masiva mediante un archivo ZIP que contiene los documentos de la solicitud.</li></ul>";
        }


        public struct FirmaInternaResultado
        {
            public const string Summary = "Esperar el resultado del API-FIRD";

            public const string Description = ResumenHtml +
                 "<ul><li>Permite registrar y procesar el resultado devuelto por el servicio API-FIRD para actualizar el estado de los documentos y de la solicitud.</li></ul>";
        }

        public struct FirmaExterna
        {
            public const string Summary = "Firma externa";

            public const string Description = ResumenHtml +
                "<ul><li>Permite iniciar el proceso de firma externa para los documentos asociados a una solicitud.</li></ul>";
        }


    }


}
