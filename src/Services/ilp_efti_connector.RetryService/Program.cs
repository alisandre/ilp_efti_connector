using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.RetryService;
using ilp_efti_connector.RetryService.Consumers;
using ilp_efti_connector.Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<RetryEftiMessageConsumer>();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
