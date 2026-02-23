using ilp_efti_connector.Application.DTOs;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Queries.GetCustomerByCode;

public sealed class GetCustomerByCodeQueryHandler
    : IRequestHandler<GetCustomerByCodeQuery, CustomerDto?>
{
    private readonly ICustomerRepository _customers;

    public GetCustomerByCodeQueryHandler(ICustomerRepository customers)
        => _customers = customers;

    public async Task<CustomerDto?> Handle(GetCustomerByCodeQuery query, CancellationToken ct)
    {
        var customer = await _customers.GetByCodeAsync(query.CustomerCode, ct);
        if (customer is null) return null;

        return new CustomerDto(
            customer.Id,
            customer.CustomerCode,
            customer.BusinessName,
            customer.VatNumber,
            customer.EoriCode,
            customer.ContactEmail,
            customer.IsActive,
            customer.AutoCreated,
            customer.SourceId,
            customer.CreatedAt,
            customer.UpdatedAt);
    }
}
