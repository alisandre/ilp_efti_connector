using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.EftiNative.Auth;

/// <summary>Risposta standard OAuth2 token endpoint (RFC 6749).</summary>
internal sealed class EftiTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } = 3600;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";
}
