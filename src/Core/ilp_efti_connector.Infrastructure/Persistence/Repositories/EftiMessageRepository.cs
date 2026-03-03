using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.Infrastructure.Persistence.Repositories;

public sealed class EftiMessageRepository : IEftiMessageRepository
{
    private readonly EftiConnectorDbContext _db;

    public EftiMessageRepository(EftiConnectorDbContext db) => _db = db;

    public async Task<EftiMessage?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.EftiMessages.FindAsync([id], ct);

    public async Task<IReadOnlyList<EftiMessage>> GetByTransportOperationIdAsync(
        Guid transportOperationId, CancellationToken ct = default)
        => await _db.EftiMessages
            .Where(m => m.TransportOperationId == transportOperationId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EftiMessage>> GetByStatusAsync(
        MessageStatus status, int page, int pageSize, CancellationToken ct = default)
        => await _db.EftiMessages
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EftiMessage>> GetPendingForRetryAsync(
        DateTime utcNow, CancellationToken ct = default)
        => await _db.EftiMessages
            .Where(m => m.Status == MessageStatus.RETRY
                     && m.NextRetryAt.HasValue
                     && m.NextRetryAt.Value <= utcNow)
            .OrderBy(m => m.NextRetryAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EftiMessage>> GetStuckPendingAsync(
        TimeSpan stuckThreshold, CancellationToken ct = default)
        => await _db.EftiMessages
            .Where(m => m.Status == MessageStatus.PENDING
                     && m.CreatedAt <= DateTime.UtcNow - stuckThreshold)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(EftiMessage message, CancellationToken ct = default)
        => await _db.EftiMessages.AddAsync(message, ct);

    public void Update(EftiMessage message)
        => _db.EftiMessages.Update(message);
}
