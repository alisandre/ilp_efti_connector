using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddIlpEftiRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName   = "ilp_efti_";
        });

        return services;
    }
}
