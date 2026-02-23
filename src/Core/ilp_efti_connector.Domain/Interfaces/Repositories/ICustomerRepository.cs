using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Domain.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByCodeAsync(string customerCode, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(bool activeOnly = false, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAutoCreatedAsync(CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
    void Update(Customer customer);
}
