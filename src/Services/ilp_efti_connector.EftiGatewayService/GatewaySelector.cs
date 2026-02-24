using ilp_efti_connector.Gateway.Contracts;
using ilp_efti_connector.Gateway.EftiNative;
using ilp_efti_connector.Gateway.EftiNative.Client;
using ilp_efti_connector.Gateway.Milos;
using ilp_efti_connector.Gateway.Milos.Client;
using Microsoft.Extensions.Logging;

namespace ilp_efti_connector.EftiGatewayService;

/// <summary>
/// Risolve l'implementazione corretta di <see cref="IEftiGateway"/>
/// in base al nome del provider (MILOS | EFTI_NATIVE).
/// </summary>
public sealed class GatewaySelector
{
    private readonly IReadOnlyDictionary<string, IEftiGateway> _gateways;

    public GatewaySelector(
        IMilosEcmrClient         milosClient,
        ILogger<MilosTfpGateway> milosLogger,
        IEftiGateClient          eftiClient,
        ILogger<EftiNativeGateway> eftiLogger)
    {
        _gateways = new Dictionary<string, IEftiGateway>(StringComparer.OrdinalIgnoreCase)
        {
            ["MILOS"]        = new MilosTfpGateway(milosClient, milosLogger),
            ["EFTI_NATIVE"]  = new EftiNativeGateway(eftiClient, eftiLogger),
        };
    }

    public IEftiGateway Get(string providerName) =>
        _gateways.TryGetValue(providerName, out var gw)
            ? gw
            : throw new NotSupportedException($"Gateway provider '{providerName}' non supportato.");
}
