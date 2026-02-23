using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Domain.Interfaces.Repositories;

public interface ICustomerDestinationRepository
{
    Task<CustomerDestination?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CustomerDestination?> GetByCodeAsync(string destinationCode, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerDestination>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<CustomerDestination?> GetDefaultForCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(CustomerDestination destination, CancellationToken ct = default);
    void Update(CustomerDestination destination);
}
