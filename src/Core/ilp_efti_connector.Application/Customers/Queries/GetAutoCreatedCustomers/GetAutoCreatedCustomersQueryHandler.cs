using ilp_efti_connector.Application.DTOs;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Queries.GetAutoCreatedCustomers;

public sealed class GetAutoCreatedCustomersQueryHandler
    : IRequestHandler<GetAutoCreatedCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerRepository _customers;

    public GetAutoCreatedCustomersQueryHandler(ICustomerRepository customers)
        => _customers = customers;

    public async Task<IReadOnlyList<CustomerDto>> Handle(
        GetAutoCreatedCustomersQuery query, CancellationToken ct)
    {
        var list = await _customers.GetAutoCreatedAsync(ct);

        return list.Select(c => new CustomerDto(
            c.Id, c.CustomerCode, c.BusinessName, c.VatNumber, c.EoriCode,
            c.ContactEmail, c.IsActive, c.AutoCreated, c.SourceId,
            c.CreatedAt, c.UpdatedAt))
            .ToList();
    }
}
