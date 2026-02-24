using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.EftiNative.Models.EN17532;

public sealed class EftiConsignee
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("playerType")]
    public string? PlayerType { get; set; }

    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }

    [JsonPropertyName("eoriCode")]
    public string? EoriCode { get; set; }

    [JsonPropertyName("address")]
    public EftiAddress? Address { get; set; }
}
