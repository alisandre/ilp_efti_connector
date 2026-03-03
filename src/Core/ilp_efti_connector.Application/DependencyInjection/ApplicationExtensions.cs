using FluentValidation;
using ilp_efti_connector.Application.Common.Behaviours;
using ilp_efti_connector.Application.Common.Identity;
using ilp_efti_connector.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.Application.DependencyInjection;

/// <summary>
/// Registrazione centralizzata del layer Application:
/// MediatR (con tutti i pipeline behaviour), FluentValidation e <see cref="ICurrentUserService"/>.
/// Ogni servizio chiama questo metodo invece di <c>AddMediatR</c> direttamente.
/// I servizi web sovrascrivono <see cref="ICurrentUserService"/> chiamando
/// <c>AddIlpEftiAuth</c> (che registra <c>HttpContextCurrentUserService</c>).
/// </summary>
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationExtensions).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<ICurrentUserService, NullCurrentUserService>();

        return services;
    }
}
