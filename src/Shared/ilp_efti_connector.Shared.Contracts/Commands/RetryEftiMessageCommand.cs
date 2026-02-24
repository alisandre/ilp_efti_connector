namespace ilp_efti_connector.Shared.Contracts.Commands;

/// <summary>
/// Comando per forzare il retry manuale di un messaggio EFTI fallito.
/// Può essere inviato dall'operatore tramite la Dashboard Dead Letter Queue.
/// </summary>
public record RetryEftiMessageCommand(
    Guid   EftiMessageId,
    string RequestedBy
);
