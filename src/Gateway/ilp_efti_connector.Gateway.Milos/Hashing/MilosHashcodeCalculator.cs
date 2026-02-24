using ilp_efti_connector.Gateway.Milos.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.Hashing;

/// <summary>
/// Calcola l'hash SHA-256 del payload ECMRRequest (senza il campo hashcodeDetails)
/// da inserire in <c>hashcodeDetails.json</c> secondo l'ICD MILOS.
/// </summary>
public static class MilosHashcodeCalculator
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented               = false
    };

    /// <summary>
    /// Serializza <paramref name="request"/> senza il campo <c>hashcodeDetails</c>
    /// e restituisce l'hash SHA-256 in hex lowercase.
    /// </summary>
    public static HashcodeDetails Compute(ECMRRequest request)
    {
        var saved = request.HashcodeDetails;
        request.HashcodeDetails = null;

        try
        {
            var json      = JsonSerializer.Serialize(request, _options);
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
            return new HashcodeDetails
            {
                Json      = Convert.ToHexString(hashBytes).ToLowerInvariant(),
                Algorithm = "SHA-256"
            };
        }
        finally
        {
            request.HashcodeDetails = saved;
        }
    }
}
