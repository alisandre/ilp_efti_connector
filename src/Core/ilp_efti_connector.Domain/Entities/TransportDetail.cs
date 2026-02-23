using ilp_efti_connector.Domain.ValueObjects;

namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Dettagli di trasporto dell'operazione: incoterms, cargoType e luoghi contrattuali.
/// Mappa la tabella 'transport_details' (relazione 1:1 con transport_operations).
/// Corrisponde ai campi 'transportDetails', 'contractualCarrierAcceptanceLocation'
/// e 'contractualConsigneeReceiptLocation' del modello MILOS ECMRRequest.
/// </summary>
public class TransportDetail
{
    public Guid Id { get; set; }

    /// <summary>FK verso transport_operations (relazione 1:1).</summary>
    public Guid TransportOperationId { get; set; }

    /// <summary>Modalità di carico (cargoType MILOS: FTL, LTL, GROUPAGE).</summary>
    public Enums.CargoType? CargoType { get; set; }

    /// <summary>Termini di resa Incoterms (incoterms MILOS).</summary>
    public Enums.Incoterms? Incoterms { get; set; }

    /// <summary>
    /// Luogo contrattuale di presa in carico da parte del vettore
    /// (contractualCarrierAcceptanceLocation.postalAddress MILOS).
    /// </summary>
    public PostalAddress AcceptanceAddress { get; set; } = new();

    /// <summary>
    /// Data/ora contrattuale di presa in carico
    /// (contractualCarrierAcceptanceLocation.date MILOS).
    /// </summary>
    public DateTime? AcceptanceDate { get; set; }

    /// <summary>
    /// Luogo contrattuale di consegna al destinatario
    /// (contractualConsigneeReceiptLocation.postalAddress MILOS).
    /// </summary>
    public PostalAddress ReceiptAddress { get; set; } = new();

    // Navigation property
    public TransportOperation TransportOperation { get; set; } = null!;
}
