using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.Infrastructure.Persistence.Repositories;

public sealed class TransportOperationRepository : ITransportOperationRepository
{
    private readonly EftiConnectorDbContext _db;

    public TransportOperationRepository(EftiConnectorDbContext db) => _db = db;

    public async Task<TransportOperation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.TransportOperations.FindAsync([id], ct);

    public async Task<TransportOperation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _db.TransportOperations
            .Include(o => o.Customer)
            .Include(o => o.Destination)
            .Include(o => o.Source)
            .Include(o => o.Consignee)
            .Include(o => o.Detail)
            .Include(o => o.ConsignmentItem)
                .ThenInclude(ci => ci!.Packages)
            .Include(o => o.Carriers)
            .Include(o => o.EftiMessages)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<TransportOperation?> GetByCodeAsync(string operationCode, CancellationToken ct = default)
        => await _db.TransportOperations
            .FirstOrDefaultAsync(o => o.OperationCode == operationCode, ct);

    public async Task<bool> ExistsByCodeAsync(string operationCode, CancellationToken ct = default)
        => await _db.TransportOperations
            .AnyAsync(o => o.OperationCode == operationCode, ct);

    public async Task AddAsync(TransportOperation operation, CancellationToken ct = default)
        => await _db.TransportOperations.AddAsync(operation, ct);

    public void Update(TransportOperation operation)
        => _db.TransportOperations.Update(operation);
}
