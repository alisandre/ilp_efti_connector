using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.ResponseHandlerService.Consumers;
using ilp_efti_connector.Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<EftiResponseReceivedConsumer>();
});

var host = builder.Build();
host.Run();
