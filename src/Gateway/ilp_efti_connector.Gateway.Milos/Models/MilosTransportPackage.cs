using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

public sealed class MilosTransportPackage
{
    [JsonPropertyName("shippingMarks")]
    public string? ShippingMarks { get; set; }

    [JsonPropertyName("itemQuantity")]
    public int ItemQuantity { get; set; }

    [JsonPropertyName("typeCode")]
    public string? TypeCode { get; set; }

    [JsonPropertyName("grossWeight")]
    public decimal GrossWeight { get; set; }

    [JsonPropertyName("grossVolume")]
    public decimal? GrossVolume { get; set; }
}
