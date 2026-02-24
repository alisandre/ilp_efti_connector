using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

/// <summary>Sezione shipping dell'ECMRRequest — identifica il documento.</summary>
public sealed class Shipping
{
    [JsonPropertyName("eCMRID")]
    public string ECMRId { get; set; } = string.Empty;

    [JsonPropertyName("datasetType")]
    public string DatasetType { get; set; } = string.Empty;
}
