using System.Text.Json;
using COFIDE.WFF.Architecture.Tests.Support;

namespace COFIDE.WFF.Architecture.Tests.Reglas;

[Trait("Categoria", "BuenasPracticas")]
public class BuenasPracticasTests
{
    [Fact, Trait("Hallazgo", "B13")]
    public void No_se_usa_dynamic()
    {
        ArchitectureAssert.SinViolaciones(
            "B13 - Sin dynamic",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(), Codigo.Patron(@"\bdynamic\b")),
            "Use el tipo concreto para que el compilador verifique las llamadas.");
    }

    [Fact, Trait("Hallazgo", "B6")]
    public void No_se_escribe_en_consola()
    {
        ArchitectureAssert.SinViolaciones(
            "B6 - Sin Console.Write",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(), Codigo.Patron(@"\bConsole\.(Write|WriteLine)\b")),
            "Use ILogger; elimine el código de depuración.");
    }

    [Fact, Trait("Hallazgo", "B7")]
    public void Transaccion_no_se_decide_por_el_nombre_del_tipo()
    {
        var patron = Codigo.Patron(@"\.Name\.EndsWith\(\s*""(Command|Query)""");

        ArchitectureAssert.SinViolaciones(
            "B7 - Transacción por interfaz marcadora",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(Capas.Application), patron),
            "Use una interfaz marcadora (ITransactionalCommand) en lugar del sufijo del nombre.");
    }

    [Fact, Trait("Hallazgo", "B5")]
    public void Application_no_captura_Exception_generica()
    {
        var patron = Codigo.Patron(@"catch\s*\(\s*(System\.)?Exception\b");

        ArchitectureAssert.SinViolaciones(
            "B5 - Sin catch (Exception) en Application",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(Capas.Application), patron),
            "Capture excepciones específicas; los reintentos de notificaciones los debe asumir el Outbox.");
    }

    [Fact, Trait("Categoria", "Hexagonal"), Trait("Hallazgo", "H2")]
    public void Application_no_accede_al_sistema_de_archivos()
    {
        var patron = Codigo.Patron(@"\bPath\.Combine\b|\bFile\.(Exists|Read\w*|Write\w*|Delete|Open\w*|Copy|Move)\b|\bDirectory\.\w+");

        ArchitectureAssert.SinViolaciones(
            "H2 - Application sin sistema de archivos",
            Codigo.Coincidencias(RutasSolucion.ArchivosCs(Capas.Application), patron),
            "Delegue rutas y lectura/escritura en IFileStorageService (Infraestructura).");
    }

    [Fact, Trait("Hallazgo", "B8")]
    public void HttpClients_tienen_politica_de_resiliencia()
    {
        var violaciones = new List<string>();
        var alta = Codigo.Patron(@"\.AddHttpClient\s*[<(]");
        var resiliencia = Codigo.Patron(@"\.Add(Standard)?Resilience\w*\(|\.AddPolicyHandler\(|\.AddTransientHttpErrorPolicy\(");

        foreach (var archivo in RutasSolucion.ArchivosCs())
        {
            var lineas = archivo.LineasDeCodigo();
            var clientes = lineas.Count(l => alta.IsMatch(l.Texto));
            var politicas = lineas.Count(l => resiliencia.IsMatch(l.Texto));
            if (clientes > politicas)
                violaciones.Add($"{archivo.RutaRelativa}: {clientes} HttpClient(s) y {politicas} política(s) de resiliencia");
        }

        ArchitectureAssert.SinViolaciones(
            "B8 - Resiliencia HTTP",
            violaciones,
            "Encadene .AddStandardResilienceHandler() (Microsoft.Extensions.Http.Resilience) a cada AddHttpClient.");
    }

    [Fact, Trait("Hallazgo", "B12")]
    public void Existe_politica_de_autorizacion_por_defecto()
    {
        var patron = Codigo.Patron(@"FallbackPolicy\s*=");
        var hayFallback = RutasSolucion.ArchivosCs(Capas.Api).Concat(RutasSolucion.ArchivosCs(Capas.Infrastructure))
            .Any(a => a.LineasDeCodigo().Any(l => patron.IsMatch(l.Texto)));

        ArchitectureAssert.SinViolaciones(
            "B12 - FallbackPolicy de autorización",
            hayFallback ? Enumerable.Empty<string>() : new[] { "No se encontró 'FallbackPolicy =' en Api ni Infrastructure" },
            "Configure FallbackPolicy = RequireAuthenticatedUser y marque con [AllowAnonymous] solo lo público.");
    }

    [Fact, Trait("Hallazgo", "B11")]
    public void Appsettings_base_sin_secretos_ni_valores_de_desarrollo()
    {
        var claveSensible = Codigo.Patron(@"(secret|password|pwd|apikey|api_key|clientsecret|privatekey)", ignorarMayusculas: true);
        var valorDesarrollo = Codigo.Patron(@"(\bdev\.|apigwdev|hubdev|webdev|\\DEV\\|localhost)", ignorarMayusculas: true);
        var violaciones = new List<string>();

        foreach (var proyecto in RutasSolucion.ProyectosSrc())
        {
            var ruta = Path.Combine(RutasSolucion.CarpetaProyecto(proyecto), "appsettings.json");
            if (!File.Exists(ruta)) continue;

            using var doc = JsonDocument.Parse(File.ReadAllText(ruta),
                new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

            foreach (var (clave, valor) in Aplanar(doc.RootElement, ""))
            {
                if (string.IsNullOrWhiteSpace(valor)) continue;
                var nombreClave = clave.Split(':').Last();
                if (claveSensible.IsMatch(nombreClave))
                    violaciones.Add($"{proyecto}/appsettings.json: '{clave}' tiene valor (posible secreto)");
                if (valorDesarrollo.IsMatch(valor))
                    violaciones.Add($"{proyecto}/appsettings.json: '{clave}' apunta a desarrollo");
            }
        }

        ArchitectureAssert.SinViolaciones(
            "B11 - appsettings.json base",
            violaciones,
            "Deje valores neutros en appsettings.json, mueva lo de DEV a appsettings.Development.json y los secretos a variables de entorno.");
    }

    private static IEnumerable<(string Clave, string Valor)> Aplanar(JsonElement elemento, string prefijo)
    {
        switch (elemento.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in elemento.EnumerateObject())
                    foreach (var x in Aplanar(p.Value, prefijo.Length == 0 ? p.Name : $"{prefijo}:{p.Name}"))
                        yield return x;
                break;
            case JsonValueKind.Array:
                var i = 0;
                foreach (var item in elemento.EnumerateArray())
                    foreach (var x in Aplanar(item, $"{prefijo}:{i++}"))
                        yield return x;
                break;
            case JsonValueKind.String:
                yield return (prefijo, elemento.GetString() ?? "");
                break;
        }
    }
}
