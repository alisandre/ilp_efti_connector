using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registra health checks per MariaDB e Redis.
    /// RabbitMQ è coperto automaticamente dall'health check nativo di MassTransit
    /// (registrato da <see cref="MessagingExtensions.AddIlpEftiMessaging"/>).
    /// </summary>
    public static IServiceCollection AddIlpEftiHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeRedis = true)
    {
        var checks = services.AddHealthChecks();

        var dbConn = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(dbConn))
            checks.AddMySql(dbConn, name: "mariadb", tags: ["db", "ready"]);

        if (includeRedis)
        {
            var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            checks.AddRedis(redisConn, name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: ["cache", "ready"]);
        }

        return services;
    }
}
