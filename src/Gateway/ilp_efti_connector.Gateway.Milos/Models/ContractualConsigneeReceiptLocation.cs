using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

public sealed class ContractualConsigneeReceiptLocation
{
    [JsonPropertyName("postalAddress")]
    public PostalAddress PostalAddress { get; set; } = new();
}
