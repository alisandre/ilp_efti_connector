using ilp_efti_connector.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MariaDb;

namespace ilp_efti_connector.IntegrationTests.Infrastructure;

/// <summary>
/// Fixture condivisa (IAsyncLifetime) che avvia un container MariaDB 11.4 tramite Testcontainers,
/// applica le EF Core migrations e restituisce un IServiceProvider pronto per i test.
/// Usata come [Collection] xUnit per evitare più container per suite.
/// </summary>
public sealed class MariaDbContainerFixture : IAsyncLifetime
{
    private readonly MariaDbContainer _container = new MariaDbBuilder()
        .WithImage("mariadb:11.4")
        .WithDatabase("efti_test")
        .WithUsername("efti_test")
        .WithPassword("efti_test_pwd")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;
    public IServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();

        var services = new ServiceCollection();
        services.AddDbContext<EftiConnectorDbContext>(options =>
            options.UseMySql(
                ConnectionString,
                new MariaDbServerVersion(new Version(11, 4, 0))));

        Services = services.BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Services is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Crea un nuovo scope DI isolato per ogni test, evitando contaminazioni tra test.
    /// </summary>
    public IServiceScope CreateScope() => Services.CreateScope();
}

[CollectionDefinition(CollectionName)]
public sealed class MariaDbCollection : ICollectionFixture<MariaDbContainerFixture>
{
    public const string CollectionName = "MariaDb Integration";
}
