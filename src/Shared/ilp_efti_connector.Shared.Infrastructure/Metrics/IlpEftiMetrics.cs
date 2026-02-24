using System.Diagnostics.Metrics;

namespace ilp_efti_connector.Shared.Infrastructure.Metrics;

/// <summary>
/// Metriche Prometheus custom per l'EFTI Connector.
/// Registrate nel meter "<see cref="MeterName"/>" ed esposte tramite OpenTelemetry.
/// </summary>
public static class IlpEftiMetrics
{
    public const string MeterName = "ilp_efti_connector";

    private static readonly Meter _meter = new(MeterName, "1.0.0");

    /// <summary>Numero totale di operazioni di trasporto ricevute dall'API Gateway.</summary>
    public static readonly Counter<long> TransportOperationsSubmitted =
        _meter.CreateCounter<long>(
            "transport_operations_submitted_total",
            description: "Operazioni di trasporto ricevute dall'API Gateway.");

    /// <summary>Numero totale di messaggi EFTI inviati al gateway.</summary>
    public static readonly Counter<long> EftiMessagesSent =
        _meter.CreateCounter<long>(
            "efti_messages_sent_total",
            description: "Messaggi EFTI inviati al gateway (MILOS o EFTI Native).");

    /// <summary>Numero totale di messaggi EFTI con ACK ricevuto.</summary>
    public static readonly Counter<long> EftiMessagesAcknowledged =
        _meter.CreateCounter<long>(
            "efti_messages_acknowledged_total",
            description: "Messaggi EFTI con ACK finale ricevuto dal gateway.");

    /// <summary>Numero totale di messaggi EFTI in errore.</summary>
    public static readonly Counter<long> EftiMessagesFailed =
        _meter.CreateCounter<long>(
            "efti_messages_failed_total",
            description: "Messaggi EFTI che hanno prodotto un errore.");

    /// <summary>Numero totale di retry effettuati su messaggi EFTI.</summary>
    public static readonly Counter<long> EftiMessagesRetried =
        _meter.CreateCounter<long>(
            "efti_messages_retried_total",
            description: "Tentativi di reinvio su messaggi EFTI falliti.");

    /// <summary>Durata in millisecondi delle richieste verso il gateway esterno.</summary>
    public static readonly Histogram<double> GatewayRequestDuration =
        _meter.CreateHistogram<double>(
            "gateway_request_duration_ms",
            unit: "ms",
            description: "Durata delle chiamate HTTP verso MILOS o EFTI Gate.");

    /// <summary>Numero di messaggi in stato DEAD (Dead Letter Queue).</summary>
    public static readonly Counter<long> EftiMessagesDead =
        _meter.CreateCounter<long>(
            "efti_messages_dead_total",
            description: "Messaggi EFTI finiti in Dead Letter Queue dopo i tentativi massimi.");
}
