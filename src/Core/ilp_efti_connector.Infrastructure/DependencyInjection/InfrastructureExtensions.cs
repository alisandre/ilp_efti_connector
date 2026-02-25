using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Infrastructure.Persistence;
using ilp_efti_connector.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.Infrastructure.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EftiConnectorDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                new MariaDbServerVersion(new Version(11, 4, 0)),
                mysql => mysql.EnableRetryOnFailure(3)));

        services.AddScoped<ISourceRepository, SourceRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerDestinationRepository, CustomerDestinationRepository>();
        services.AddScoped<ITransportOperationRepository, TransportOperationRepository>();
        services.AddScoped<IEftiMessageRepository, EftiMessageRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
