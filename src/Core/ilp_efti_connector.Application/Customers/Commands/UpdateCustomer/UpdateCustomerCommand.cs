using ilp_efti_connector.Application.Common.Interfaces;
using ilp_efti_connector.Domain.Enums;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string BusinessName,
    string? VatNumber,
    string? EoriCode,
    string? ContactEmail,
    bool IsActive
) : IRequest, IAuditableCommandWithEntityId
{
    public AuditEntityType EntityType       => AuditEntityType.Customer;
    public AuditActionType ActionType       => AuditActionType.Update;
    public string          AuditDescription => $"Aggiornamento cliente [{Id}]";
    public Guid            EntityId         => Id;
}
