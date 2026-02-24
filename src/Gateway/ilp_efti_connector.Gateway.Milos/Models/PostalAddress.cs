using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

/// <summary>Indirizzo postale nel formato MILOS.</summary>
public sealed class PostalAddress
{
    [JsonPropertyName("streetName")]
    public string? StreetName { get; set; }

    [JsonPropertyName("postCode")]
    public string? PostCode { get; set; }

    [JsonPropertyName("cityName")]
    public string CityName { get; set; } = string.Empty;

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("countryName")]
    public string? CountryName { get; set; }
}
