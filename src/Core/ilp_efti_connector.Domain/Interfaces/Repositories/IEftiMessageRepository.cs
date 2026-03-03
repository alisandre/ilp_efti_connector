using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Domain.Interfaces.Repositories;

public interface IEftiMessageRepository
{
    Task<EftiMessage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EftiMessage>> GetByTransportOperationIdAsync(Guid transportOperationId, CancellationToken ct = default);
    Task<IReadOnlyList<EftiMessage>> GetByStatusAsync(MessageStatus status, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<EftiMessage>> GetPendingForRetryAsync(DateTime utcNow, CancellationToken ct = default);
    /// <summary>Messaggi PENDING mai processati, fermi da più di <paramref name="stuckThreshold"/> (EftiSendRequestedEvent andato perso).</summary>
    Task<IReadOnlyList<EftiMessage>> GetStuckPendingAsync(TimeSpan stuckThreshold, CancellationToken ct = default);
    Task AddAsync(EftiMessage message, CancellationToken ct = default);
    void Update(EftiMessage message);
}
