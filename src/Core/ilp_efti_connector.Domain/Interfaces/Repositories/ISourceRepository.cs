using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Domain.Interfaces.Repositories;

public interface ISourceRepository
{
    Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
