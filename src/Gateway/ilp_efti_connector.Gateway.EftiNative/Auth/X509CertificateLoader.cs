using System.Security.Cryptography.X509Certificates;

namespace ilp_efti_connector.Gateway.EftiNative.Auth;

/// <summary>
/// Carica un certificato X.509 da file (.pfx / .p12) per mutual TLS verso l'EFTI Gate.
/// </summary>
public static class X509CertificateLoader
{
    public static X509Certificate2 Load(string path, string? password = null)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Certificato X.509 non trovato: {path}");

        return System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
            path,
            password.AsSpan(),
            X509KeyStorageFlags.MachineKeySet);
    }
}
