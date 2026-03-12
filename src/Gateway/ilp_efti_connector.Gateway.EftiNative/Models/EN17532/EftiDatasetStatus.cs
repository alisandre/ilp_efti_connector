using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.EftiNative.Models.EN17532;

/// <summary>
/// Risposta all'endpoint <c>GET /datasets/{id}/status</c> dell'EFTI Gate nazionale.
/// Usata come meccanismo di fallback al callback asincrono.
/// </summary>
public sealed class EftiDatasetStatus
{
    /// <summary>UID eFTI del dataset nel formato CC.PlatformId.DatasetType.UniqueId.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Stato attuale del dataset nel Gate.
    /// Valori possibili: RECEIVED | PROCESSING | VALIDATED | FORWARDED | ACKNOWLEDGED | ERROR
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Messaggio di errore (popolato solo se status = ERROR).</summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>Codice errore strutturato dal Gate (es. SCHEMA_VALIDATION_FAILED).</summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    /// <summary>Timestamp dell'ultimo aggiornamento di stato (ISO 8601).</summary>
    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    /// <summary>Timestamp di ricezione iniziale del dataset (ISO 8601).</summary>
    [JsonPropertyName("receivedAt")]
    public string? ReceivedAt { get; set; }

    // ── Helpers ──────────────────────────────────────────────────────────────

    public bool IsTerminal =>
        Status is "ACKNOWLEDGED" or "ERROR";

    public bool IsAcknowledged =>
        Status == "ACKNOWLEDGED";

    public bool IsError =>
        Status == "ERROR";
}
