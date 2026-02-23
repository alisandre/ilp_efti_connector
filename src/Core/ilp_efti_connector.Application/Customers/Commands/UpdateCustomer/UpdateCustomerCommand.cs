using MediatR;

namespace ilp_efti_connector.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string BusinessName,
    string? VatNumber,
    string? EoriCode,
    string? ContactEmail,
    bool IsActive
) : IRequest;
