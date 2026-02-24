using ilp_efti_connector.Shared.Infrastructure.Extensions;
using ilp_efti_connector.ValidationService.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<TransportSubmittedConsumer>();
});

var host = builder.Build();
host.Run();
