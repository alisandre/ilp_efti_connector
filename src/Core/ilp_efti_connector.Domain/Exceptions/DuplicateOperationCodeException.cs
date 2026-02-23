namespace ilp_efti_connector.Domain.Exceptions;

public sealed class DuplicateOperationCodeException : DomainException
{
    public DuplicateOperationCodeException(string operationCode)
        : base($"Esiste già un'operazione di trasporto con codice '{operationCode}'.") { }
}
