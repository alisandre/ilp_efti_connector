using ilp_efti_connector.Gateway.Contracts.Models;

namespace ilp_efti_connector.Gateway.Contracts;

/// <summary>
/// Interfaccia comune per i gateway EFTI. Invariata tra Fase 1 (MILOS) e Fase 2 (EFTI Native).
/// La selezione dell'implementazione attiva avviene tramite feature flag in appsettings.json.
/// </summary>
public interface IEftiGateway
{
    /// <summary>Invia un nuovo e-CMR / e-DDT al gateway attivo.</summary>
    Task<EftiSendResult> SendEcmrAsync(EcmrPayload payload, CancellationToken ct = default);

    /// <summary>Aggiorna un e-CMR / e-DDT esistente identificato da <paramref name="externalId"/>.</summary>
    Task<EftiSendResult> UpdateEcmrAsync(string externalId, EcmrPayload payload, CancellationToken ct = default);

    /// <summary>Cancella un e-CMR / e-DDT esistente identificato da <paramref name="externalId"/>.</summary>
    Task<EftiSendResult> DeleteEcmrAsync(string externalId, CancellationToken ct = default);

    /// <summary>Recupera i dati di un e-CMR / e-DDT dal gateway.</summary>
    Task<EcmrPayload> GetEcmrAsync(string externalId, CancellationToken ct = default);

    /// <summary>Verifica la raggiungibilità del gateway. Restituisce <c>true</c> se disponibile.</summary>
    Task<GatewayHealthStatus> HealthCheckAsync(CancellationToken ct = default);
}
