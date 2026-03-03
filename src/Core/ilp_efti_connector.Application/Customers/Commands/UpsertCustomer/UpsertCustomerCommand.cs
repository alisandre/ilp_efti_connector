using ilp_efti_connector.Application.Common.Interfaces;
using ilp_efti_connector.Domain.Enums;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Commands.UpsertCustomer;

/// <summary>
/// Esegue l'upsert di cliente e destinazione a partire dai dati ricevuti dalla sorgente.
/// Corrisponde alla logica descritta in sezione 4.2 dell'architettura.
/// </summary>
public sealed record UpsertCustomerCommand(
    string CustomerCode,
    string BusinessName,
    string? VatNumber,
    string? EoriCode,
    Guid SourceId,
    // Destinazione (opzionale — se il payload la include)
    string? DestinationCode,
    string? AddressLine1,
    string? City,
    string? PostalCode,
    string? Province,
    string? CountryCode,
    string? UnLocode
) : IRequest<UpsertCustomerResult>, IAuditableCommand
{
    public AuditEntityType EntityType       => AuditEntityType.Customer;
    public AuditActionType ActionType       => AuditActionType.Update;
    public string          AuditDescription => $"Upsert cliente [{CustomerCode}]";
}

public sealed record UpsertCustomerResult(
    Guid CustomerId,
    Guid? DestinationId,
    bool IsNewCustomer,
    bool IsNewDestination
) : IAuditableResult
{
    public Guid AuditEntityId => CustomerId;
}
