using ilp_efti_connector.Domain.ValueObjects;

namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Vettore/trasportatore dell'operazione di trasporto.
/// Mappa la tabella 'transport_carriers' (relazione 1:N con transport_operations).
/// Corrisponde a un elemento dell'array 'carriers[]' del modello MILOS ECMRRequest.
/// </summary>
public class TransportCarrier
{
    public Guid Id { get; set; }

    /// <summary>FK verso transport_operations.</summary>
    public Guid TransportOperationId { get; set; }

    /// <summary>Ordine del vettore nell'array (1-based).</summary>
    public int SortOrder { get; set; } = 1;

    /// <summary>Ragione sociale del vettore.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ruolo nell'operazione (tipicamente CARRIER).</summary>
    public Enums.PlayerType PlayerType { get; set; } = Enums.PlayerType.CARRIER;

    /// <summary>Indirizzo postale del vettore (Owned Type).</summary>
    public PostalAddress PostalAddress { get; set; } = new();

    /// <summary>Partita IVA / numero fiscale (taxRegistration MILOS).</summary>
    public string? TaxRegistration { get; set; }

    /// <summary>Codice EORI del vettore.</summary>
    public string? EoriCode { get; set; }

    /// <summary>Targa del trattore/mezzo (tractorPlate MILOS — obbligatorio).</summary>
    public string TractorPlate { get; set; } = string.Empty;

    /// <summary>Categoria dell'equipaggiamento/mezzo (equipmentCategory MILOS).</summary>
    public Enums.EcmrEquipmentCategory? EquipmentCategory { get; set; }

    // Navigation property
    public TransportOperation TransportOperation { get; set; } = null!;
}
