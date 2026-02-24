namespace ilp_efti_connector.FormInputService.Models;

/// <summary>Risposta alla sottomissione di un'operazione di trasporto via form.</summary>
public sealed record FormSubmitResponse(
    Guid   TransportOperationId,
    string CorrelationId,
    string Status
);

/// <summary>Stato dettagliato di un'operazione per la dashboard React.</summary>
public sealed record FormOperationStatusResponse(
    Guid      TransportOperationId,
    string    OperationCode,
    string    DatasetType,
    string    Status,
    string?   GatewayProvider,
    string?   ExternalId,
    short     RetryCount,
    DateTime? SentAt,
    DateTime? AcknowledgedAt,
    DateTime  CreatedAt,
    DateTime  UpdatedAt
);
