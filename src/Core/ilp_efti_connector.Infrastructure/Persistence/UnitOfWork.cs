using ilp_efti_connector.Domain.Interfaces.Repositories;

namespace ilp_efti_connector.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly EftiConnectorDbContext _db;

    public UnitOfWork(EftiConnectorDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
