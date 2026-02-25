using ilp_efti_connector.Gateway.Milos.Hashing;
using ilp_efti_connector.Gateway.Milos.Models;

namespace ilp_efti_connector.Gateway.Milos.Tests;

public sealed class MilosHashcodeCalculatorTests
{
    [Fact]
    public void Compute_ShouldReturn_ValidSha256Hex()
    {
        var result = MilosHashcodeCalculator.Compute(BuildRequest("TEST-001"));

        result.Should().NotBeNull();
        result.Algorithm.Should().Be("SHA-256");
        result.Json.Should().HaveLength(64).And.MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Compute_ShouldBe_Deterministic()
    {
        var hash1 = MilosHashcodeCalculator.Compute(BuildRequest("TEST-001"));
        var hash2 = MilosHashcodeCalculator.Compute(BuildRequest("TEST-001"));

        hash1.Json.Should().Be(hash2.Json);
    }

    [Fact]
    public void Compute_ShouldChange_WhenPayloadChanges()
    {
        var hash1 = MilosHashcodeCalculator.Compute(BuildRequest("TEST-001"));
        var hash2 = MilosHashcodeCalculator.Compute(BuildRequest("TEST-002"));

        hash1.Json.Should().NotBe(hash2.Json);
    }

    [Fact]
    public void Compute_ShouldNotModify_ExistingHashcodeDetails()
    {
        var request = BuildRequest("TEST-001");
        request.HashcodeDetails = new HashcodeDetails { Json = "original-hash", Algorithm = "SHA-256" };

        MilosHashcodeCalculator.Compute(request);

        request.HashcodeDetails.Json.Should().Be("original-hash");
    }

    [Fact]
    public void Compute_ShouldExclude_HashcodeDetails_FromInput()
    {
        var request = BuildRequest("TEST-001");

        var hashWithNoExisting  = MilosHashcodeCalculator.Compute(request);
        request.HashcodeDetails = new HashcodeDetails { Json = "some-other-hash" };
        var hashWithExisting    = MilosHashcodeCalculator.Compute(request);

        hashWithNoExisting.Json.Should().Be(hashWithExisting.Json,
            "hashcodeDetails non deve influenzare il calcolo dell'hash");
    }

    private static ECMRRequest BuildRequest(string ecmrId) => new()
    {
        Shipping        = new Shipping { ECMRId = ecmrId, DatasetType = "ECMR" },
        ConsignorSender = new Player   { Name = "Mittente SRL", TaxRegistration = "IT12345678901" },
        Consignee       = new Player   { Name = "Destinatario SRL" },
        Carriers        = [new Carrier { Name = "Vettore SRL", TractorPlate = "AB123CD" }]
    };
}
