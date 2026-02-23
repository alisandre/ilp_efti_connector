using ilp_efti_connector.Application.DTOs;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Queries.GetAutoCreatedCustomers;

public sealed record GetAutoCreatedCustomersQuery : IRequest<IReadOnlyList<CustomerDto>>;
