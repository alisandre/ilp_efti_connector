using ilp_efti_connector.Domain.Exceptions;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MediatR;

namespace ilp_efti_connector.Application.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _uow;

    public UpdateCustomerCommandHandler(ICustomerRepository customers, IUnitOfWork uow)
    {
        _customers = customers;
        _uow       = uow;
    }

    public async Task Handle(UpdateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(cmd.Id, ct)
            ?? throw new CustomerNotFoundException(cmd.Id);

        customer.BusinessName = cmd.BusinessName;
        customer.VatNumber    = cmd.VatNumber;
        customer.EoriCode     = cmd.EoriCode;
        customer.ContactEmail = cmd.ContactEmail;
        customer.IsActive     = cmd.IsActive;
        customer.UpdatedAt    = DateTime.UtcNow;

        _customers.Update(customer);
        await _uow.SaveChangesAsync(ct);
    }
}
