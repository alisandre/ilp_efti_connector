using ilp_efti_connector.Application.DependencyInjection;
using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.NormalizationService.Consumers;
using ilp_efti_connector.Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<TransportValidatedConsumer>();
});

var host = builder.Build();
host.Run();
