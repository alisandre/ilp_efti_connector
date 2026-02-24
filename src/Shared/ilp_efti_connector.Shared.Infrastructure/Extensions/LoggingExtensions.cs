using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Configura Serilog con enrichers, lettura da <c>appsettings.json</c> e sink opzionale verso Seq.
    /// Da invocare su <see cref="IHostBuilder"/> prima di <c>Build()</c>.
    /// </summary>
    public static IHostBuilder AddIlpEftiLogging(
        this IHostBuilder builder,
        string serviceName)
    {
        builder.UseSerilog((ctx, services, config) =>
        {
            config
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ServiceName", serviceName)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System",    LogEventLevel.Warning);

            var seqUrl = ctx.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
                config.WriteTo.Seq(seqUrl);
            else
                config.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");
        });

        return builder;
    }
}
