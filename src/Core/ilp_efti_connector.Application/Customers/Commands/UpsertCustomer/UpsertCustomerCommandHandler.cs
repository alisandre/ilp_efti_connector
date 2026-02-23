using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Commands.UpsertCustomer;

public sealed class UpsertCustomerCommandHandler
    : IRequestHandler<UpsertCustomerCommand, UpsertCustomerResult>
{
    private readonly ICustomerRepository _customers;
    private readonly ICustomerDestinationRepository _destinations;
    private readonly IUnitOfWork _uow;

    public UpsertCustomerCommandHandler(
        ICustomerRepository customers,
        ICustomerDestinationRepository destinations,
        IUnitOfWork uow)
    {
        _customers    = customers;
        _destinations = destinations;
        _uow          = uow;
    }

    public async Task<UpsertCustomerResult> Handle(
        UpsertCustomerCommand cmd, CancellationToken ct)
    {
        // 1. Lookup / crea cliente
        var customer = await _customers.GetByCodeAsync(cmd.CustomerCode, ct);
        bool isNewCustomer = customer is null;

        if (customer is null)
        {
            customer = new Customer
            {
                Id           = Guid.NewGuid(),
                CustomerCode = cmd.CustomerCode,
                BusinessName = cmd.BusinessName,
                VatNumber    = cmd.VatNumber,
                EoriCode     = cmd.EoriCode,
                SourceId     = cmd.SourceId,
                AutoCreated  = true,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };
            await _customers.AddAsync(customer, ct);
        }
        else
        {
            bool changed =
                customer.BusinessName != cmd.BusinessName ||
                customer.VatNumber    != cmd.VatNumber    ||
                customer.EoriCode     != cmd.EoriCode;

            if (changed)
            {
                customer.BusinessName = cmd.BusinessName;
                customer.VatNumber    = cmd.VatNumber;
                customer.EoriCode     = cmd.EoriCode;
                customer.UpdatedAt    = DateTime.UtcNow;
                _customers.Update(customer);
            }
        }

        // 2. Lookup / crea destinazione (se fornita)
        Guid? destinationId = null;
        bool isNewDestination = false;

        if (!string.IsNullOrWhiteSpace(cmd.DestinationCode) &&
            !string.IsNullOrWhiteSpace(cmd.City) &&
            !string.IsNullOrWhiteSpace(cmd.CountryCode))
        {
            var destination = await _destinations.GetByCodeAsync(cmd.DestinationCode, ct);
            isNewDestination = destination is null;

            if (destination is null)
            {
                destination = new CustomerDestination
                {
                    Id              = Guid.NewGuid(),
                    CustomerId      = customer.Id,
                    DestinationCode = cmd.DestinationCode,
                    AddressLine1    = cmd.AddressLine1 ?? string.Empty,
                    City            = cmd.City,
                    PostalCode      = cmd.PostalCode,
                    Province        = cmd.Province,
                    CountryCode     = cmd.CountryCode,
                    UnLocode        = cmd.UnLocode,
                    AutoCreated     = true,
                    CreatedAt       = DateTime.UtcNow,
                    UpdatedAt       = DateTime.UtcNow
                };
                await _destinations.AddAsync(destination, ct);
            }
            else
            {
                bool changed =
                    destination.AddressLine1 != (cmd.AddressLine1 ?? string.Empty) ||
                    destination.City         != cmd.City                             ||
                    destination.PostalCode   != cmd.PostalCode                       ||
                    destination.CountryCode  != cmd.CountryCode;

                if (changed)
                {
                    destination.AddressLine1 = cmd.AddressLine1 ?? string.Empty;
                    destination.City         = cmd.City;
                    destination.PostalCode   = cmd.PostalCode;
                    destination.Province     = cmd.Province;
                    destination.CountryCode  = cmd.CountryCode;
                    destination.UnLocode     = cmd.UnLocode;
                    destination.UpdatedAt    = DateTime.UtcNow;
                    _destinations.Update(destination);
                }
            }

            destinationId = destination.Id;
        }

        await _uow.SaveChangesAsync(ct);

        return new UpsertCustomerResult(
            customer.Id, destinationId, isNewCustomer, isNewDestination);
    }
}
