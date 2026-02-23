namespace ilp_efti_connector.Domain.Exceptions;

public sealed class CustomerNotFoundException : DomainException
{
    public CustomerNotFoundException(string customerCode)
        : base($"Cliente non trovato con codice '{customerCode}'.") { }

    public CustomerNotFoundException(Guid id)
        : base($"Cliente non trovato con ID '{id}'.") { }
}
