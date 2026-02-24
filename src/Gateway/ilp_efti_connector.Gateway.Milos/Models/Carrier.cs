using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Models;

/// <summary>
/// Vettore — estende Player con dati del mezzo.
/// MILOS type value: CARRIER
/// </summary>
public sealed class Carrier : Player
{
    [JsonPropertyName("tractorPlate")]
    public string? TractorPlate { get; set; }

    [JsonPropertyName("trailerPlate")]
    public string? TrailerPlate { get; set; }

    [JsonPropertyName("tractorPlateCountryCode")]
    public string? TractorPlateCountryCode { get; set; }

    [JsonPropertyName("trailerPlateCountryCode")]
    public string? TrailerPlateCountryCode { get; set; }

    /// <summary>
    /// Categoria equipaggiamento MILOS.
    /// Values: CONTAINER | SEMITRAILER | TRAILER | SWAP_BODY | TANK | FLAT_RACK | VAN | REEFER | OTHER …
    /// </summary>
    [JsonPropertyName("equipmentCategory")]
    public string? EquipmentCategory { get; set; }
}
