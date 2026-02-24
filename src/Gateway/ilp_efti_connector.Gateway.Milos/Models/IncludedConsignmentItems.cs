using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

public sealed class IncludedConsignmentItems
{
    [JsonPropertyName("transportPackages")]
    public List<MilosTransportPackage> TransportPackages { get; set; } = [];

    [JsonPropertyName("totalItemQuantity")]
    public int TotalItemQuantity { get; set; }

    [JsonPropertyName("totalWeight")]
    public decimal TotalWeight { get; set; }

    [JsonPropertyName("totalVolume")]
    public decimal? TotalVolume { get; set; }
}
