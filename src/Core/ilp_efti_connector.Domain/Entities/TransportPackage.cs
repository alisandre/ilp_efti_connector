namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Singolo collo/unità di imballaggio della spedizione.
/// Mappa la tabella 'transport_packages' (relazione 1:N con transport_consignment_items).
/// Corrisponde a un elemento di 'includedConsignmentItems.transportPackages[]' MILOS.
/// </summary>
public class TransportPackage
{
    public Guid Id { get; set; }

    /// <summary>FK verso transport_consignment_items.</summary>
    public Guid ConsignmentItemId { get; set; }

    /// <summary>Ordine del collo nell'array (1-based).</summary>
    public int SortOrder { get; set; } = 1;

    /// <summary>Marcatura/codice del collo (shippingMarks MILOS).</summary>
    public string? ShippingMarks { get; set; }

    /// <summary>Quantità di unità nel collo (itemQuantity MILOS).</summary>
    public int ItemQuantity { get; set; }

    /// <summary>Tipo di imballaggio (typeCode MILOS — es. PALLET, BOX).</summary>
    public string? TypeCode { get; set; }

    /// <summary>Peso lordo in kg (grossWeight MILOS).</summary>
    public decimal GrossWeight { get; set; }

    /// <summary>Volume lordo in m³ (grossVolume MILOS).</summary>
    public decimal? GrossVolume { get; set; }

    // Navigation property
    public TransportConsignmentItem ConsignmentItem { get; set; } = null!;
}
