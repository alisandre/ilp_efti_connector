namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Totali della spedizione (includedConsignmentItems MILOS).
/// Mappa la tabella 'transport_consignment_items' (relazione 1:1 con transport_operations).
/// </summary>
public class TransportConsignmentItem
{
    public Guid Id { get; set; }

    /// <summary>FK verso transport_operations (relazione 1:1).</summary>
    public Guid TransportOperationId { get; set; }

    /// <summary>Numero totale colli (totalItemQuantity MILOS).</summary>
    public int TotalItemQuantity { get; set; }

    /// <summary>Peso lordo totale in kg (totalWeight MILOS).</summary>
    public decimal TotalWeight { get; set; }

    /// <summary>Volume totale in m³ (totalVolume MILOS).</summary>
    public decimal? TotalVolume { get; set; }

    // Navigation properties
    public TransportOperation TransportOperation { get; set; } = null!;
    public ICollection<TransportPackage> Packages { get; set; } = new List<TransportPackage>();
}
