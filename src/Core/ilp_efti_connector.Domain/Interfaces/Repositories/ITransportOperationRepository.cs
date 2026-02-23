using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Domain.Interfaces.Repositories;

public interface ITransportOperationRepository
{
    Task<TransportOperation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TransportOperation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<TransportOperation?> GetByCodeAsync(string operationCode, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string operationCode, CancellationToken ct = default);
    Task AddAsync(TransportOperation operation, CancellationToken ct = default);
    void Update(TransportOperation operation);
}
