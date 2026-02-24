using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Registra MassTransit con RabbitMQ. Il delegate <paramref name="configureConsumers"/>
    /// consente a ciascun microservizio di registrare i propri consumer.
    /// </summary>
    public static IServiceCollection AddIlpEftiMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMQ:Host"] ?? "localhost",
                    configuration["RabbitMQ:VirtualHost"] ?? "/",
                    h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
