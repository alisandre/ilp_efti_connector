using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly EftiConnectorDbContext _db;

    public CustomerRepository(EftiConnectorDbContext db) => _db = db;

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Customers.FindAsync([id], ct);

    public async Task<Customer?> GetByCodeAsync(string customerCode, CancellationToken ct = default)
        => await _db.Customers
            .FirstOrDefaultAsync(c => c.CustomerCode == customerCode, ct);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(bool activeOnly = false, CancellationToken ct = default)
    {
        var query = _db.Customers.AsQueryable();
        if (activeOnly)
            query = query.Where(c => c.IsActive);
        return await query.OrderBy(c => c.BusinessName).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetAutoCreatedAsync(CancellationToken ct = default)
        => await _db.Customers
            .Where(c => c.AutoCreated)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
        => await _db.Customers.AddAsync(customer, ct);

    public void Update(Customer customer)
        => _db.Customers.Update(customer);
}
