using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.NormalizationService.Consumers;
using ilp_efti_connector.Shared.Infrastructure.Extensions;
using MediatR;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(ilp_efti_connector.Application.Customers.Commands.UpsertCustomer.UpsertCustomerCommand).Assembly));

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<TransportValidatedConsumer>();
});

var host = builder.Build();
host.Run();
