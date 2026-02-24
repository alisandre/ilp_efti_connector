using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

/// <summary>
/// Soggetto generico nel trasporto (consignorSender, consignee).
/// MILOS type values: CONSIGNOR_SENDER | SELLER | CONSIGNEE | FREIGHT_FORWARDER
/// </summary>
public class Player
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("postalAddress")]
    public PostalAddress PostalAddress { get; set; } = new();

    [JsonPropertyName("taxRegistration")]
    public string? TaxRegistration { get; set; }

    [JsonPropertyName("EORICode")]
    public string? EORICode { get; set; }
}
