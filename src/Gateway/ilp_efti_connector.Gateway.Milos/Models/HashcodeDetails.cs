using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

public sealed class HashcodeDetails
{
    [JsonPropertyName("json")]
    public string Json { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "SHA-256";
}
