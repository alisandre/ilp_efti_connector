using System.Text.RegularExpressions;

namespace ilp_efti_connector.Gateway.EftiNative;

/// <summary>
/// Genera identificatori univoci eFTI conformi al formato definito dal Reference Implementation
/// eFTI4EU e dal Regolamento EU 2020/1056.
/// <para>
/// Formato: <c>{CC}.{PlatformId}.{DatasetType}.{UniqueId}</c><br/>
/// Esempio: <c>IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123</c>
/// </para>
/// </summary>
public static class EftiUidGenerator
{
    // Solo alfanumerici, trattini e underscore sono ammessi nei segmenti UID
    private static readonly Regex _invalidChars = new(@"[^A-Za-z0-9\-_]", RegexOptions.Compiled);

    /// <summary>
    /// Genera un UID eFTI nel formato <c>{CC}.{PlatformId}.{DatasetType}.{UniqueId}</c>.
    /// </summary>
    /// <param name="countryCode">Codice paese ISO 3166-1 alpha-2 (es. IT).</param>
    /// <param name="platformId">Identificatore della piattaforma TFP (es. EFTI_CONNECTOR).</param>
    /// <param name="datasetType">Tipo documento (es. ECMR, EAWB, EBL).</param>
    /// <param name="operationCode">Codice univoco dell'operazione dalla piattaforma sorgente.</param>
    public static string Generate(
        string countryCode,
        string platformId,
        string datasetType,
        string operationCode)
    {
        var cc       = Sanitize(countryCode).ToUpperInvariant();
        var platform = Sanitize(platformId).ToUpperInvariant();
        var type     = Sanitize(datasetType).ToUpperInvariant();
        var unique   = Sanitize(operationCode);

        return $"{cc}.{platform}.{type}.{unique}";
    }

    /// <summary>
    /// Estrae l'<c>operationCode</c> originale da un UID eFTI completo.
    /// Restituisce l'UID invariato se non è nel formato atteso.
    /// </summary>
    public static string ExtractOperationCode(string eftiUid)
    {
        var parts = eftiUid.Split('.', 4);
        return parts.Length == 4 ? parts[3] : eftiUid;
    }

    private static string Sanitize(string value) =>
        _invalidChars.Replace(value ?? string.Empty, "_");
}
