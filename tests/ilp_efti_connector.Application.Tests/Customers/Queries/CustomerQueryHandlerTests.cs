using ilp_efti_connector.Application.Customers.Queries.GetAutoCreatedCustomers;
using ilp_efti_connector.Application.Customers.Queries.GetCustomerByCode;
using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;

namespace ilp_efti_connector.Application.Tests.Customers.Queries;

public sealed class CustomerQueryHandlerTests
{
    private readonly Mock<ICustomerRepository> _customers = new();

    // ─── GetCustomerByCodeQueryHandler ────────────────────────────────────────

    [Fact]
    public async Task GetByCode_ExistingCustomer_ReturnsDto()
    {
        var customer = BuildCustomer("CUST-001", "Test SRL");
        _customers.Setup(r => r.GetByCodeAsync("CUST-001", default)).ReturnsAsync(customer);

        var sut    = new GetCustomerByCodeQueryHandler(_customers.Object);
        var result = await sut.Handle(new GetCustomerByCodeQuery("CUST-001"), default);

        result.Should().NotBeNull();
        result!.CustomerCode.Should().Be("CUST-001");
        result.BusinessName.Should().Be("Test SRL");
    }

    [Fact]
    public async Task GetByCode_NonExistingCustomer_ReturnsNull()
    {
        _customers.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), default)).ReturnsAsync((Customer?)null);

        var sut    = new GetCustomerByCodeQueryHandler(_customers.Object);
        var result = await sut.Handle(new GetCustomerByCodeQuery("NOT-FOUND"), default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCode_ShouldMap_AllFields()
    {
        var customer = BuildCustomer("CUST-002", "Mittente SRL", vat: "IT12345678901", eori: "EORI001");
        _customers.Setup(r => r.GetByCodeAsync("CUST-002", default)).ReturnsAsync(customer);

        var sut    = new GetCustomerByCodeQueryHandler(_customers.Object);
        var result = await sut.Handle(new GetCustomerByCodeQuery("CUST-002"), default);

        result!.VatNumber.Should().Be("IT12345678901");
        result.EoriCode.Should().Be("EORI001");
        result.IsActive.Should().BeTrue();
        result.AutoCreated.Should().BeTrue();
    }

    // ─── GetAutoCreatedCustomersQueryHandler ──────────────────────────────────

    [Fact]
    public async Task GetAutoCreated_ReturnsOnlyAutoCreatedCustomers()
    {
        var list = new List<Customer>
        {
            BuildCustomer("AC-001", "Auto SRL"),
            BuildCustomer("AC-002", "Auto Due SRL")
        };
        _customers.Setup(r => r.GetAutoCreatedAsync(default)).ReturnsAsync(list);

        var sut    = new GetAutoCreatedCustomersQueryHandler(_customers.Object);
        var result = await sut.Handle(new GetAutoCreatedCustomersQuery(), default);

        result.Should().HaveCount(2);
        result.All(c => c.AutoCreated).Should().BeTrue();
    }

    [Fact]
    public async Task GetAutoCreated_EmptyList_ReturnsEmptyCollection()
    {
        _customers.Setup(r => r.GetAutoCreatedAsync(default)).ReturnsAsync([]);

        var sut    = new GetAutoCreatedCustomersQueryHandler(_customers.Object);
        var result = await sut.Handle(new GetAutoCreatedCustomersQuery(), default);

        result.Should().BeEmpty();
    }

    // ─── Factory ─────────────────────────────────────────────────────────────

    private static Customer BuildCustomer(
        string code,
        string name,
        string? vat  = null,
        string? eori = null) => new()
    {
        Id           = Guid.NewGuid(),
        CustomerCode = code,
        BusinessName = name,
        VatNumber    = vat,
        EoriCode     = eori,
        IsActive     = true,
        AutoCreated  = true,
        SourceId     = Guid.NewGuid()
    };
}
