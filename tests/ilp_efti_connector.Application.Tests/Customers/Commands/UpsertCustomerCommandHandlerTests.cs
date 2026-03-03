using ilp_efti_connector.Application.Customers.Commands.UpsertCustomer;
using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;

namespace ilp_efti_connector.Application.Tests.Customers.Commands;

public sealed class UpsertCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository>            _customers    = new();
    private readonly Mock<ICustomerDestinationRepository> _destinations = new();
    private readonly Mock<IUnitOfWork>                    _uow          = new();
    private readonly UpsertCustomerCommandHandler         _sut;

    public UpsertCustomerCommandHandlerTests()
        => _sut = new UpsertCustomerCommandHandler(_customers.Object, _destinations.Object, _uow.Object);

    // ─── Nuovo cliente ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NewCustomer_ShouldAdd_AndReturnIsNewCustomerTrue()
    {
        _customers.Setup(r => r.GetByCodeAsync("CUST-001", default)).ReturnsAsync((Customer?)null);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.Handle(BuildCmd("CUST-001"), default);

        result.IsNewCustomer.Should().BeTrue();
        _customers.Verify(r => r.AddAsync(It.IsAny<Customer>(), default), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_NewCustomer_ShouldSet_AutoCreatedTrue()
    {
        _customers.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), default)).ReturnsAsync((Customer?)null);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        Customer? captured = null;
        _customers.Setup(r => r.AddAsync(It.IsAny<Customer>(), default))
                  .Callback<Customer, CancellationToken>((c, _) => captured = c);

        await _sut.Handle(BuildCmd("CUST-NEW"), default);

        captured!.AutoCreated.Should().BeTrue();
        captured.IsActive.Should().BeTrue();
    }

    // ─── Cliente esistente ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExistingCustomer_WhenDataChanged_ShouldUpdate()
    {
        var existing = new Customer
        {
            Id           = Guid.NewGuid(),
            CustomerCode = "CUST-002",
            BusinessName = "Vecchio Nome SRL",
            VatNumber    = null
        };
        _customers.Setup(r => r.GetByCodeAsync("CUST-002", default)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var cmd    = BuildCmd("CUST-002", businessName: "Nuovo Nome SRL", vat: "IT99999999999");
        var result = await _sut.Handle(cmd, default);

        result.IsNewCustomer.Should().BeFalse();
        _customers.Verify(r => r.Update(It.Is<Customer>(c => c.BusinessName == "Nuovo Nome SRL")), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingCustomer_WhenDataUnchanged_ShouldNotUpdate()
    {
        var existing = new Customer
        {
            Id           = Guid.NewGuid(),
            CustomerCode = "CUST-003",
            BusinessName = "Stessa Ragione SRL",
            VatNumber    = "IT12345678901"
        };
        _customers.Setup(r => r.GetByCodeAsync("CUST-003", default)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(0);

        await _sut.Handle(BuildCmd("CUST-003", businessName: "Stessa Ragione SRL", vat: "IT12345678901"), default);

        _customers.Verify(r => r.Update(It.IsAny<Customer>()), Times.Never);
    }

    // ─── Destinazione nuova ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NewDestination_ShouldAdd_AndReturnIsNewDestinationTrue()
    {
        _customers.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), default)).ReturnsAsync((Customer?)null);
        _destinations.Setup(r => r.GetByCodeAsync("DEST-001", default)).ReturnsAsync((CustomerDestination?)null);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var cmd = BuildCmd("CUST-004", destinationCode: "DEST-001", city: "Milano", country: "IT");

        var result = await _sut.Handle(cmd, default);

        result.IsNewDestination.Should().BeTrue();
        _destinations.Verify(r => r.AddAsync(It.IsAny<CustomerDestination>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDestinationCodeMissing_ShouldNotCreateDestination()
    {
        _customers.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), default)).ReturnsAsync((Customer?)null);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.Handle(BuildCmd("CUST-005"), default);

        result.DestinationId.Should().BeNull();
        _destinations.Verify(r => r.AddAsync(It.IsAny<CustomerDestination>(), default), Times.Never);
    }

    // ─── Factory ─────────────────────────────────────────────────────────────

    private static UpsertCustomerCommand BuildCmd(
        string code,
        string businessName   = "Test SRL",
        string? vat           = null,
        string? destinationCode = null,
        string? city          = null,
        string? country       = null) => new(
        CustomerCode:   code,
        BusinessName:   businessName,
        VatNumber:      vat,
        EoriCode:       null,
        SourceId:       Guid.NewGuid(),
        DestinationCode:destinationCode,
        AddressLine1:   null,
        City:           city,
        PostalCode:     null,
        Province:       null,
        CountryCode:    country,
        UnLocode:       null);
}
