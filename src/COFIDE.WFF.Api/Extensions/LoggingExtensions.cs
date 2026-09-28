using COFIDE.WFF.Api.Configuration;
using Serilog;
using Serilog.Events;
namespace COFIDE.WFF.Api.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection("SerilogOptions")
            .Get<SerilogOptions>()
            ?? throw new InvalidOperationException("SerilogOptions no configurado");

        if (string.IsNullOrWhiteSpace(options.InfoLogPath) ||
            string.IsNullOrWhiteSpace(options.ErrorLogPath))
        {
            throw new InvalidOperationException("Rutas de log no configuradas correctamente.");
        }

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<LogEventLevel>(options.MinimumLevel, true))

            // EF Core
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)

            // ASP.NET Core
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            //.MinimumLevel.Override("System", LogEventLevel.Warning)

            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", "COFIDE.WFF.API");


            ConfigureFileSink(
                loggerConfig,
                options.InfoLogPath,
                LogEventLevel.Information,
                options.RetainDaysInfo,
                options.OutputTemplate);

            ConfigureFileSink(
                loggerConfig,
                options.ErrorLogPath,
                LogEventLevel.Error,
                options.RetainDaysError,
                options.OutputTemplate);

        if (options.EnableConsole)
        {
            loggerConfig.WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: options.OutputTemplate
            );
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
    }

    private static void ConfigureFileSink(
    LoggerConfiguration loggerConfig,
    string path,
    LogEventLevel level,
    int retainDays,
    string outputTemplate)
    {
        loggerConfig.WriteTo.Async(a => a.File(
            path,
            restrictedToMinimumLevel: level,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: retainDays,
            shared: true,
            outputTemplate: outputTemplate
        ));
    }
}
