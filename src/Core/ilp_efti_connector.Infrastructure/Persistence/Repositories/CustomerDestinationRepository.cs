using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.Infrastructure.Persistence.Repositories;

public sealed class CustomerDestinationRepository : ICustomerDestinationRepository
{
    private readonly EftiConnectorDbContext _db;

    public CustomerDestinationRepository(EftiConnectorDbContext db) => _db = db;

    public async Task<CustomerDestination?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.CustomerDestinations.FindAsync([id], ct);

    public async Task<CustomerDestination?> GetByCodeAsync(string destinationCode, CancellationToken ct = default)
        => await _db.CustomerDestinations
            .FirstOrDefaultAsync(d => d.DestinationCode == destinationCode, ct);

    public async Task<IReadOnlyList<CustomerDestination>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => await _db.CustomerDestinations
            .Where(d => d.CustomerId == customerId)
            .OrderBy(d => d.City)
            .ToListAsync(ct);

    public async Task<CustomerDestination?> GetDefaultForCustomerAsync(Guid customerId, CancellationToken ct = default)
        => await _db.CustomerDestinations
            .FirstOrDefaultAsync(d => d.CustomerId == customerId && d.IsDefault, ct);

    public async Task AddAsync(CustomerDestination destination, CancellationToken ct = default)
        => await _db.CustomerDestinations.AddAsync(destination, ct);

    public void Update(CustomerDestination destination)
        => _db.CustomerDestinations.Update(destination);
}
