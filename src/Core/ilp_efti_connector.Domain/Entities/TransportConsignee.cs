using ilp_efti_connector.Domain.ValueObjects;

namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Destinatario dell'operazione di trasporto.
/// Mappa la tabella 'transport_consignees' (relazione 1:1 con transport_operations).
/// Corrisponde al campo 'consignee' del modello MILOS ECMRRequest.
/// </summary>
public class TransportConsignee
{
    public Guid Id { get; set; }

    /// <summary>FK verso transport_operations (relazione 1:1).</summary>
    public Guid TransportOperationId { get; set; }

    /// <summary>Ragione sociale del destinatario.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ruolo nell'operazione (tipicamente CONSIGNEE).</summary>
    public Enums.PlayerType PlayerType { get; set; } = Enums.PlayerType.CONSIGNEE;

    /// <summary>Indirizzo postale del destinatario (Owned Type).</summary>
    public PostalAddress PostalAddress { get; set; } = new();

    /// <summary>Partita IVA / numero fiscale (taxRegistration MILOS).</summary>
    public string? TaxRegistration { get; set; }

    /// <summary>Codice EORI del destinatario.</summary>
    public string? EoriCode { get; set; }

    // Navigation property
    public TransportOperation TransportOperation { get; set; } = null!;
}
