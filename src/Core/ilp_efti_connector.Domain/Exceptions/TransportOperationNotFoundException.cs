namespace ilp_efti_connector.Domain.Exceptions;

public sealed class TransportOperationNotFoundException : DomainException
{
    public TransportOperationNotFoundException(Guid id)
        : base($"Operazione di trasporto non trovata con ID '{id}'.") { }

    public TransportOperationNotFoundException(string operationCode)
        : base($"Operazione di trasporto non trovata con codice '{operationCode}'.") { }
}
