using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

/// <summary>Risposta da POST /ecmr — contiene eCMRID e uuid assegnati da MILOS.</summary>
public sealed class ECMRResponse
{
    [JsonPropertyName("eCMRID")]
    public string ECMRId { get; set; } = string.Empty;

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("consignorSender")]
    public Player? ConsignorSender { get; set; }
}
