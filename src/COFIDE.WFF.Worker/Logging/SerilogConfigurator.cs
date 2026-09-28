using Serilog;
using Serilog.Events;
namespace COFIDE.WFF.Worker.Logging;

public static class SerilogConfigurator
{
    public static void Configure(HostApplicationBuilder builder, string appName)
    {
        var options = builder.Configuration
            .GetSection("SerilogOptions")
            .Get<SerilogOptions>()
            ?? throw new InvalidOperationException("SerilogOptions no configurado");

        if (string.IsNullOrWhiteSpace(options.InfoLogPath) ||
            string.IsNullOrWhiteSpace(options.ErrorLogPath))
        {
            throw new InvalidOperationException("Rutas de log no configuradas.");
        }

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<LogEventLevel>(options.MinimumLevel, true))

            // EF Core
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)

            // ASP.NET Core
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)

            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", appName);

        loggerConfig.WriteTo.Async(a => a.File(
            options.InfoLogPath,
            restrictedToMinimumLevel: LogEventLevel.Information,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: options.RetainDaysInfo,
            shared: true,
            outputTemplate: options.OutputTemplate
        ));

        loggerConfig.WriteTo.Async(a => a.File(
            options.ErrorLogPath,
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: options.RetainDaysError,
            shared: true,
            outputTemplate: options.OutputTemplate
        ));

        if (options.EnableConsole)
        {
            loggerConfig.WriteTo.Console(outputTemplate: options.OutputTemplate);
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }
}
