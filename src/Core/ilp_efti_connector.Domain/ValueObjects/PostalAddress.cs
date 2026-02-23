namespace ilp_efti_connector.Domain.ValueObjects;

/// <summary>
/// Indirizzo postale riutilizzato come EF Core Owned Type.
/// Corrisponde al tipo PostalAddress del modello MILOS e del dataset EN 17532.
/// </summary>
public class PostalAddress
{
    /// <summary>Via e numero civico (streetName MILOS).</summary>
    public string? StreetName { get; set; }

    /// <summary>Codice postale / CAP (postCode MILOS).</summary>
    public string? PostCode { get; set; }

    /// <summary>Città (cityName MILOS).</summary>
    public string CityName { get; set; } = string.Empty;

    /// <summary>Codice paese ISO 3166-1 alpha-2 (countryCode MILOS).</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Nome per esteso del paese (countryName MILOS).</summary>
    public string? CountryName { get; set; }
}
