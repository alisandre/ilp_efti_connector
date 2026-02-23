using ilp_efti_connector.Application.DTOs;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Queries.GetCustomerByCode;

public sealed record GetCustomerByCodeQuery(string CustomerCode) : IRequest<CustomerDto?>;
