using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

public sealed class ContractualCarrierAcceptanceLocation
{
    [JsonPropertyName("postalAddress")]
    public PostalAddress PostalAddress { get; set; } = new();

    /// <summary>Format: "yyyy-MM-dd HH:mm:ss"</summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }
}
