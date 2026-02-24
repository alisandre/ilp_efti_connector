using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

public sealed class MilosTransportDetails
{
    /// <summary>Values: FTL | LTL | groupage</summary>
    [JsonPropertyName("cargoType")]
    public string? CargoType { get; set; }

    /// <summary>Values: EXW | FCA | CPT | CIP | DAT | DAP | DDP | FAS | FOB | CFR | CIF</summary>
    [JsonPropertyName("incoterms")]
    public string? Incoterms { get; set; }
}
