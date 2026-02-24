namespace ilp_efti_connector.Gateway.Contracts.Models;

/// <summary>
/// Risultato di un'operazione di invio verso il gateway EFTI.
/// Uniforme tra Fase 1 (MILOS) e Fase 2 (EFTI Native).
/// </summary>
/// <param name="IsSuccess">True se il gateway ha accettato il messaggio.</param>
/// <param name="ExternalId">
/// Fase 1 (MILOS): <c>eCMRID</c> dalla ECMRResponse.<br/>
/// Fase 2 (EFTI):  <c>messageId</c> dall'eFTI Gate.
/// </param>
/// <param name="ExternalUuid">
/// Fase 1 (MILOS): <c>uuid</c> dalla ECMRResponse.<br/>
/// Fase 2 (EFTI):  non utilizzato (<c>null</c>).
/// </param>
/// <param name="HttpStatusCode">Status code HTTP ricevuto dal gateway.</param>
/// <param name="ErrorCode">Codice errore restituito dal gateway in caso di fallimento.</param>
/// <param name="ErrorMessage">Messaggio descrittivo dell'errore.</param>
public record EftiSendResult(
    bool    IsSuccess,
    string? ExternalId,
    string? ExternalUuid,
    int?    HttpStatusCode,
    string? ErrorCode,
    string? ErrorMessage
)
{
    /// <summary>Crea un risultato di successo.</summary>
    public static EftiSendResult Success(string externalId, string? externalUuid = null, int httpStatusCode = 200)
        => new(true, externalId, externalUuid, httpStatusCode, null, null);

    /// <summary>Crea un risultato di fallimento.</summary>
    public static EftiSendResult Failure(string errorMessage, string? errorCode = null, int? httpStatusCode = null)
        => new(false, null, null, httpStatusCode, errorCode, errorMessage);
}
