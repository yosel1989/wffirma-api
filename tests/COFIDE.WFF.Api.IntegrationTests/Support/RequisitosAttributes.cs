namespace COFIDE.WFF.Api.IntegrationTests.Support;

/// <summary>Prueba que necesita la base de datos de pruebas. Se omite si no hay cadena de conexión.</summary>
public sealed class FactConBaseDeDatosAttribute : FactAttribute
{
    public FactConBaseDeDatosAttribute()
    {
        if (!ConfiguracionPruebas.TieneBaseDeDatos)
            Skip = "Sin cadena de conexión: complete Resources/Configuracion/conexiones.json (copia de conexiones.example.json) o defina WFF_IT_ConnectionStrings__OracleConnection.";
    }
}

/// <summary>Prueba contra servicios externos reales (SUC). Se omite salvo que se habilite explícitamente.</summary>
public sealed class FactConApisExternasAttribute : FactAttribute
{
    public FactConApisExternasAttribute()
    {
        if (!ConfiguracionPruebas.ApisExternasHabilitadas)
            Skip = "Pruebas de APIs externas deshabilitadas: active IntegrationTests:ApisExternasHabilitadas solo en QA.";
    }
}

/// <summary>Teoría que necesita la base de datos de pruebas. Se omite si no hay cadena de conexión.</summary>
public sealed class TheoryConBaseDeDatosAttribute : TheoryAttribute
{
    public TheoryConBaseDeDatosAttribute()
    {
        if (!ConfiguracionPruebas.TieneBaseDeDatos)
            Skip = "Sin cadena de conexión: complete Resources/Configuracion/conexiones.json (copia de conexiones.example.json) o defina WFF_IT_ConnectionStrings__OracleConnection.";
    }
}
