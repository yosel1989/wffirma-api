using COFIDE.WFF.Api.Configuration;
using COFIDE.WFF.Api.Extensions;
using COFIDE.WFF.Api.Middleware;
using COFIDE.WFF.Application;
using COFIDE.WFF.Infrastructure;
using COFIDE.WFF.Infrastructure.Bootstrap;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
// Serilog
builder.ConfigureSerilog();

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});


Log.Information("Iniciando...");

// RateLimit
builder.Services.AddRateLimitingConfig(builder.Configuration);

// Servicios de capas
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Versionado
builder.Services.AddVersioningConfig();

// Swagger
builder.Services.AddSwaggerConfig();

// CORS
builder.Services.AddCorsConfig(builder.Configuration);

var app = builder.Build();

app.UsePathBase("/API-WFF");

// CorrelationId
app.UseMiddleware<CorrelationMiddleware>();

// Logging 
app.UseSerilogRequestLogging();

// Swagger
app.UseSwaggerConfig();

// Middleware global de errores
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseSecurityHeaders();

// RateLimit
app.UseRateLimitingConfig();
app.UseCors(CorsConfig.DefaultPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapRateLimitedControllers();

await app.RunAsync();
