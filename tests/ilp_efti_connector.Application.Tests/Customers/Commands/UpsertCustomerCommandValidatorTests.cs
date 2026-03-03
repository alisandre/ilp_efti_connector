using ilp_efti_connector.Application.Customers.Commands.UpsertCustomer;
using FluentValidation.TestHelper;

namespace ilp_efti_connector.Application.Tests.Customers.Commands;

public sealed class UpsertCustomerCommandValidatorTests
{
    private readonly UpsertCustomerCommandValidator _validator = new();

    // ─── Valido ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(BuildValid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ─── CustomerCode ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyCustomerCode_ShouldFail()
    {
        var result = _validator.TestValidate(BuildValid() with { CustomerCode = "" });
        result.ShouldHaveValidationErrorFor(x => x.CustomerCode);
    }

    [Fact]
    public void Validate_CustomerCode_TooLong_ShouldFail()
    {
        var result = _validator.TestValidate(BuildValid() with { CustomerCode = new string('X', 101) });
        result.ShouldHaveValidationErrorFor(x => x.CustomerCode);
    }

    // ─── BusinessName ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyBusinessName_ShouldFail()
    {
        var result = _validator.TestValidate(BuildValid() with { BusinessName = "" });
        result.ShouldHaveValidationErrorFor(x => x.BusinessName);
    }

    // ─── Destinazione condizionale ────────────────────────────────────────────

    [Fact]
    public void Validate_DestinationCodeProvided_WithoutCity_ShouldFail()
    {
        var cmd = BuildValid() with { DestinationCode = "DEST-001", City = null };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Validate_DestinationCodeProvided_WithInvalidCountryCode_ShouldFail()
    {
        var cmd = BuildValid() with
        {
            DestinationCode = "DEST-001",
            City            = "Milano",
            CountryCode     = "ITA"
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void Validate_DestinationCodeProvided_WithValidCityAndCountry_ShouldPass()
    {
        var cmd = BuildValid() with
        {
            DestinationCode = "DEST-002",
            City            = "Milano",
            CountryCode     = "IT"
        };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.City);
        result.ShouldNotHaveValidationErrorFor(x => x.CountryCode);
    }

    // ─── Factory ─────────────────────────────────────────────────────────────

    private static UpsertCustomerCommand BuildValid() => new(
        CustomerCode:    "CUST-001",
        BusinessName:    "Test SRL",
        VatNumber:       null,
        EoriCode:        null,
        SourceId:        Guid.NewGuid(),
        DestinationCode: null,
        AddressLine1:    null,
        City:            null,
        PostalCode:      null,
        Province:        null,
        CountryCode:     null,
        UnLocode:        null);
}
