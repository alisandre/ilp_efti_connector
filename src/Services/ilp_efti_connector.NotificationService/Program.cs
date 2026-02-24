using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.NotificationService.Consumers;
using ilp_efti_connector.Shared.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpClient("WebhookClient")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<SourceNotificationConsumer>();
});

var host = builder.Build();
host.Run();
