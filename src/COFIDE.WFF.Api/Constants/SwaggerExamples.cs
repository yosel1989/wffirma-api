namespace COFIDE.WFF.Api.Constants;

internal static class SwaggerExamples
{
    private const string CodeStart = "<pre><code>";
    private const string CodeEnd = "</code></pre>";
    private const string NumeroPagina1 = "\"numeroPagina\": 1,\n";

    public const string ConfiguracionCrear = CodeStart +
                    "{\n" +
                    "  \"visibilidad\": false,\n" +
                    "  \"esReutilizable\": false,\n" +
                    "  \"etapas\": [\n" +
                    "    {\n" +
                    "      \"ordenEtapa\": 1,\n" +
                    "      \"nombreEtapa\": \"Etapa 1\",\n" +
                    "      \"idTipoEtapa\": 1,\n" +
                    "      \"idTipoAprobador\": 3,\n" +
                    "      \"firmantes\": [\n" +
                    "        {\n" +
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
                   CodeEnd;

    public const string ConfiguracionEditar = @"<pre><code>
{
   ...
}
</code></pre>";

    public const string TagsCrear = CodeStart +
                    "[\n" +
                    "  {\n" +
                    "    \"nombreDocumento\": \"pruebav1.pdf\",\n" +
                    NumeroPagina1 +
                    "    \"x\": 558.03,\n" +
                    "    \"y\": 19.55\n" +
                    "  },\n" +
                    "  {\n" +
                    "    \"nombreDocumento\": \"pruebav2.pdf\",\n" +
                   NumeroPagina1 +
                    "    \"x\": 558.03,\n" +
                    "    \"y\": 19.55\n" +
                    "  }\n" +
                    "]" +
                    CodeEnd;

    public const string TagsEditar = "<pre><code>" +
                   "[\n" +
                   "  {\n" +
                   "    \"id\": 10,\n" +
                   "    \"nombreDocumento\": \"pruebav11.pdf\",\n" +
                   NumeroPagina1 +
                   "    \"x\": 0,17275202696589748,\n" +
                   "    \"y\": 0,06976315146998355\n" +
                   "  },\n" +
                   "  {\n" +
                   "    \"id\": 11,\n" +
                   "    \"nombreDocumento\": \"pruebav23.pdf\",\n" +
                  NumeroPagina1 +
                   "    \"x\": 0,5705721904536359,\n" +
                   "    \"y\": 0,06448685495476973\n" +
                   "  }\n" +
                   "  {\n" +
                   "    \"id\": 0,\n" +
                   "    \"nombreDocumento\": \"pruebav35.pdf\",\n" +
                   NumeroPagina1 +
                   "    \"x\": 0,20098039215686275,\n" +
                   "    \"y\": 0,35640782057636916\n" +
                   "  }\n" +
                   "]" +
                   CodeEnd;
}