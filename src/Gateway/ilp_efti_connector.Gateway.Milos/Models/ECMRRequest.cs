using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

/// <summary>Payload principale inviato a POST /ecmr.</summary>
public sealed class ECMRRequest
{
    [JsonPropertyName("shipping")]
    public Shipping Shipping { get; set; } = new();

    [JsonPropertyName("consignorSender")]
    public Player ConsignorSender { get; set; } = new();

    [JsonPropertyName("consignee")]
    public Player Consignee { get; set; } = new();

    [JsonPropertyName("carriers")]
    public List<Carrier> Carriers { get; set; } = [];

    [JsonPropertyName("contractualCarrierAcceptanceLocation")]
    public ContractualCarrierAcceptanceLocation? ContractualCarrierAcceptanceLocation { get; set; }

    [JsonPropertyName("contractualConsigneeReceiptLocation")]
    public ContractualConsigneeReceiptLocation? ContractualConsigneeReceiptLocation { get; set; }

    [JsonPropertyName("includedConsignmentItems")]
    public IncludedConsignmentItems? IncludedConsignmentItems { get; set; }

    [JsonPropertyName("transportDetails")]
    public MilosTransportDetails? TransportDetails { get; set; }

    [JsonPropertyName("hashcodeDetails")]
    public HashcodeDetails? HashcodeDetails { get; set; }
}
