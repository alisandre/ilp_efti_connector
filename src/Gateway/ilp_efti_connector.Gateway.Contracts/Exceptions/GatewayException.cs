namespace ilp_efti_connector.Gateway.Contracts.Exceptions;

/// <summary>
/// Eccezione base per tutti gli errori originati dai gateway EFTI (MILOS o EFTI Native).
/// </summary>
public class GatewayException : Exception
{
    public string Provider { get; }
    public int?   HttpStatusCode { get; }

    public GatewayException(string provider, string message, int? httpStatusCode = null)
        : base(message)
    {
        Provider       = provider;
        HttpStatusCode = httpStatusCode;
    }

    public GatewayException(string provider, string message, Exception inner, int? httpStatusCode = null)
        : base(message, inner)
    {
        Provider       = provider;
        HttpStatusCode = httpStatusCode;
    }
}
