using ilp_efti_connector.EftiGatewayService;
using ilp_efti_connector.EftiGatewayService.Consumers;
using ilp_efti_connector.Gateway.EftiNative.DependencyInjection;
using ilp_efti_connector.Gateway.Milos.DependencyInjection;
using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMilosGateway(builder.Configuration);
builder.Services.AddEftiNativeGateway(builder.Configuration);
builder.Services.AddScoped<GatewaySelector>();

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<EftiSendRequestedConsumer>();
    x.AddConsumer<SendToGatewayConsumer>();
});

var host = builder.Build();
host.Run();
