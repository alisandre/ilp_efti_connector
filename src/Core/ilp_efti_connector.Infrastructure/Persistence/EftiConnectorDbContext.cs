using Microsoft.EntityFrameworkCore;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence;

/// <summary>
/// DbContext principale per il database EFTI Connector.
/// Gestisce tutte le entità del modello dati su MariaDB.
/// </summary>
public class EftiConnectorDbContext : DbContext
{
    public EftiConnectorDbContext(DbContextOptions<EftiConnectorDbContext> options)
        : base(options)
    {
    }

    // DbSets per tutte le entità
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerDestination> CustomerDestinations => Set<CustomerDestination>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<TransportOperation> TransportOperations => Set<TransportOperation>();
    public DbSet<TransportConsignee> TransportConsignees => Set<TransportConsignee>();
    public DbSet<TransportCarrier> TransportCarriers => Set<TransportCarrier>();
    public DbSet<TransportDetail> TransportDetails => Set<TransportDetail>();
    public DbSet<TransportConsignmentItem> TransportConsignmentItems => Set<TransportConsignmentItem>();
    public DbSet<TransportPackage> TransportPackages => Set<TransportPackage>();
    public DbSet<EftiMessage> EftiMessages => Set<EftiMessage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applica tutte le configurazioni dalle classi separate
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EftiConnectorDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aggiorna automaticamente i timestamp CreatedAt e UpdatedAt
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Customer customer)
            {
                if (entry.State == EntityState.Added)
                    customer.CreatedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is CustomerDestination destination)
            {
                if (entry.State == EntityState.Added)
                    destination.CreatedAt = DateTime.UtcNow;
                destination.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is TransportOperation operation)
            {
                if (entry.State == EntityState.Added)
                    operation.CreatedAt = DateTime.UtcNow;
                operation.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Added)
            {
                // Per le altre entità che hanno solo CreatedAt
                var createdAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProp != null && createdAtProp.CurrentValue == null)
                {
                    createdAtProp.CurrentValue = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
