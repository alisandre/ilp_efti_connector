using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.Infrastructure.Persistence.Repositories;

public sealed class SourceRepository : ISourceRepository
{
    private readonly EftiConnectorDbContext _db;

    public SourceRepository(EftiConnectorDbContext db) => _db = db;

    public Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Sources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
}
