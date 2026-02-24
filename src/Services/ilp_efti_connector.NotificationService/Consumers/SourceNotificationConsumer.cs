using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ilp_efti_connector.NotificationService.Consumers;

public sealed class SourceNotificationConsumer : IConsumer<SourceNotificationRequiredEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ISourceRepository _sources;
    private readonly ITransportOperationRepository _operations;
    private readonly IEftiMessageRepository _messages;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SourceNotificationConsumer> _logger;

    public SourceNotificationConsumer(
        ISourceRepository sources,
        ITransportOperationRepository operations,
        IEftiMessageRepository messages,
        IHttpClientFactory httpClientFactory,
        ILogger<SourceNotificationConsumer> logger)
    {
        _sources           = sources;
        _operations        = operations;
        _messages          = messages;
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    public async Task Consume(ConsumeContext<SourceNotificationRequiredEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;

        _logger.LogInformation("Notifica sorgente → OperationId={Id} Status={Status}",
            evt.TransportOperationId, evt.Status);

        var source = await _sources.GetByIdAsync(evt.SourceId, ct);
        if (source is null)
        {
            _logger.LogWarning("Source {Id} non trovata — notifica skip.", evt.SourceId);
            return;
        }

        var webhookUrl = ExtractWebhookUrl(source.ConfigJson);
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogDebug("Source {Id} non ha webhook configurato — skip.", evt.SourceId);
            return;
        }

        var msgList = await _messages.GetByTransportOperationIdAsync(evt.TransportOperationId, ct);
        var msg     = msgList.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

        var dto = new EftiMessageStatusDto(
            EftiMessageId:        msg?.Id ?? Guid.Empty,
            TransportOperationId: evt.TransportOperationId,
            OperationCode:        string.Empty,
            Status:               evt.Status,
            GatewayProvider:      msg?.GatewayProvider.ToString() ?? string.Empty,
            ExternalId:           evt.ExternalId,
            ExternalUuid:         evt.ExternalUuid,
            RetryCount:           msg?.RetryCount ?? 0,
            SentAt:               msg?.SentAt,
            AcknowledgedAt:       msg?.AcknowledgedAt,
            ErrorMessage:         evt.ErrorMessage,
            UpdatedAt:            evt.OccurredAt);

        await SendWebhookAsync(webhookUrl, dto, ct);
    }

    private async Task SendWebhookAsync(string webhookUrl, EftiMessageStatusDto dto, CancellationToken ct)
    {
        try
        {
            var client  = _httpClientFactory.CreateClient("WebhookClient");
            var content = new StringContent(
                JsonSerializer.Serialize(dto, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(webhookUrl, content, ct);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Webhook KO [{Status}] → {Url}", (int)response.StatusCode, webhookUrl);
            else
                _logger.LogInformation("Webhook OK → {Url}", webhookUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore invio webhook → {Url}", webhookUrl);
        }
    }

    private static string? ExtractWebhookUrl(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(configJson);
            return doc.RootElement.TryGetProperty("webhookUrl", out var prop)
                ? prop.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }
}
