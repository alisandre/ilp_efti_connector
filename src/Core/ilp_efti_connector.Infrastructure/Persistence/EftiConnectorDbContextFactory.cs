using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ilp_efti_connector.Infrastructure.Persistence;

/// <summary>
/// Factory usata da 'dotnet ef' a design-time per istanziare EftiConnectorDbContext
/// senza dipendere dal progetto di startup.
/// La connection string viene letta, in ordine di priorità:
///   1. Variabile d'ambiente  EFTI_CONNECTION_STRING
///   2. Stringa di default per sviluppo locale (Docker Compose)
/// </summary>
public sealed class EftiConnectorDbContextFactory : IDesignTimeDbContextFactory<EftiConnectorDbContext>
{
    public EftiConnectorDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("EFTI_CONNECTION_STRING")
            ?? "Server=localhost;Port=3306;Database=efti_connector;User=efti_user;Password=changeme_efti;";

        var optionsBuilder = new DbContextOptionsBuilder<EftiConnectorDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MariaDbServerVersion(new Version(11, 4, 0)));

        return new EftiConnectorDbContext(optionsBuilder.Options);
    }
}
