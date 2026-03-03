using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Infrastructure.Persistence;
using ilp_efti_connector.Infrastructure.Persistence.Repositories;
using ilp_efti_connector.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.IntegrationTests.Persistence;

[Collection(MariaDbCollection.CollectionName)]
public sealed class RepositoryTests(MariaDbContainerFixture fixture)
{
    private static readonly Guid SeededSourceId = new("11111111-1111-1111-1111-111111111111");

    // ── Migrations ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Migrations_ShouldApply_WithoutErrors()
    {
        using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();

        var pending = await db.Database.GetPendingMigrationsAsync();

        pending.Should().BeEmpty("tutte le migrations devono essere già applicate dalla fixture");
    }

    [Fact]
    public async Task Migrations_ShouldSeed_TestSource()
    {
        using var scope = fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();

        var source = await db.Sources.FindAsync(SeededSourceId);

        source.Should().NotBeNull();
        source!.Code.Should().Be("TMS_TEST");
        source.Type.Should().Be(SourceType.TMS);
    }

    // ── CustomerRepository ────────────────────────────────────────────────────

    [Fact]
    public async Task CustomerRepository_Add_ShouldPersist_AndGetByCode()
    {
        using var scope = fixture.CreateScope();
        var db   = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();
        var repo = new CustomerRepository(db);

        var customer = new Customer
        {
            Id           = Guid.NewGuid(),
            CustomerCode = $"TEST-CUST-{Guid.NewGuid():N}",
            BusinessName = "Test SRL",
            VatNumber    = "IT12345678901",
            IsActive     = true,
            AutoCreated  = true,
            SourceId     = SeededSourceId
        };

        await repo.AddAsync(customer);
        await db.SaveChangesAsync();

        var loaded = await repo.GetByCodeAsync(customer.CustomerCode);

        loaded.Should().NotBeNull();
        loaded!.BusinessName.Should().Be("Test SRL");
        loaded.VatNumber.Should().Be("IT12345678901");
        loaded.AutoCreated.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerRepository_Update_ShouldPersist_Changes()
    {
        using var scope = fixture.CreateScope();
        var db   = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();
        var repo = new CustomerRepository(db);

        var code = $"UPD-{Guid.NewGuid():N}";
        var customer = new Customer
        {
            Id           = Guid.NewGuid(),
            CustomerCode = code,
            BusinessName = "Vecchio Nome SRL",
            IsActive     = true,
            SourceId     = SeededSourceId
        };
        await repo.AddAsync(customer);
        await db.SaveChangesAsync();

        // detach per simulare un secondo request
        db.ChangeTracker.Clear();
        var loaded = await repo.GetByCodeAsync(code);
        loaded!.BusinessName = "Nuovo Nome SRL";
        repo.Update(loaded);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var updated = await repo.GetByCodeAsync(code);
        updated!.BusinessName.Should().Be("Nuovo Nome SRL");
    }

    // ── TransportOperationRepository ─────────────────────────────────────────

    [Fact]
    public async Task TransportOperationRepository_Add_ShouldPersist_WithEftiMessage()
    {
        using var scope = fixture.CreateScope();
        var db   = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();
        var repo = new TransportOperationRepository(db);

        var customerId = await CreateCustomerAsync(db);

        var operationCode = $"CMR-{Guid.NewGuid():N}";
        var operation = new TransportOperation
        {
            Id            = Guid.NewGuid(),
            OperationCode = operationCode,
            DatasetType   = "ECMR",
            Status        = TransportOperationStatus.SENDING,
            SourceId      = SeededSourceId,
            CustomerId    = customerId,
            EftiMessages  =
            [
                new EftiMessage
                {
                    Id              = Guid.NewGuid(),
                    GatewayProvider = GatewayProvider.MILOS,
                    Direction       = MessageDirection.OUTBOUND,
                    Status          = MessageStatus.PENDING,
                    RetryCount      = 0
                }
            ]
        };

        await repo.AddAsync(operation);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var loaded = await repo.GetByIdWithDetailsAsync(operation.Id);

        loaded.Should().NotBeNull();
        loaded!.OperationCode.Should().Be(operationCode);
        loaded.EftiMessages.Should().ContainSingle();
        loaded.EftiMessages.First().GatewayProvider.Should().Be(GatewayProvider.MILOS);
    }

    [Fact]
    public async Task TransportOperationRepository_ExistsByCode_ShouldReturn_CorrectValue()
    {
        using var scope = fixture.CreateScope();
        var db   = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();
        var repo = new TransportOperationRepository(db);

        var customerId    = await CreateCustomerAsync(db);
        var operationCode = $"EXIST-{Guid.NewGuid():N}";

        var operation = new TransportOperation
        {
            Id            = Guid.NewGuid(),
            OperationCode = operationCode,
            DatasetType   = "ECMR",
            Status        = TransportOperationStatus.DRAFT,
            SourceId      = SeededSourceId,
            CustomerId    = customerId
        };
        await repo.AddAsync(operation);
        await db.SaveChangesAsync();

        var exists    = await repo.ExistsByCodeAsync(operationCode);
        var notExists = await repo.ExistsByCodeAsync("CODICE-NON-ESISTENTE");

        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static async Task<Guid> CreateCustomerAsync(EftiConnectorDbContext db)
    {
        var customer = new Customer
        {
            Id           = Guid.NewGuid(),
            CustomerCode = $"HELPER-{Guid.NewGuid():N}",
            BusinessName = "Helper Customer SRL",
            IsActive     = true,
            SourceId     = SeededSourceId
        };
        await db.Customers.AddAsync(customer);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return customer.Id;
    }
}
