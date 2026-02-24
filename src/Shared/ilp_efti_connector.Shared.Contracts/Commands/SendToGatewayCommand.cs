namespace ilp_efti_connector.Shared.Contracts.Commands;

/// <summary>
/// Comando inviato direttamente all'EftiGatewayService per spedire un messaggio EFTI.
/// Usato dal RetryService per riprocessare messaggi in stato RETRY o DEAD.
/// </summary>
public record SendToGatewayCommand(
    Guid   EftiMessageId,
    Guid   TransportOperationId,
    string CorrelationId,
    string GatewayProvider,
    string PayloadJson,
    string DatasetType
);
