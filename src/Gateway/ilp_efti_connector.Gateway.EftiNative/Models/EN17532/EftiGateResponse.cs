namespace ilp_efti_connector.Gateway.EftiNative.Models.EN17532
{
    public class EftiGateResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string? ReasonPhrase { get; set; }
        public object? Content { get; set; }
        public EftiGateError? Error { get; set; }
    }

    public class EftiGateError
    {
        public string? Content { get; set; }
    }
}
