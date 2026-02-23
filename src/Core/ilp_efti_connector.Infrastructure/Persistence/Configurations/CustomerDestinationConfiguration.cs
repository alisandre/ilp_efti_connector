using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità CustomerDestination.
/// </summary>
public class CustomerDestinationConfiguration : IEntityTypeConfiguration<CustomerDestination>
{
    public void Configure(EntityTypeBuilder<CustomerDestination> builder)
    {
        builder.ToTable("customer_destinations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(d => d.CustomerId)
            .HasColumnName("customer_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(d => d.DestinationCode)
            .HasColumnName("destination_code")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(d => d.DestinationCode)
            .IsUnique();

        builder.Property(d => d.Label)
            .HasColumnName("label")
            .HasMaxLength(200);

        builder.Property(d => d.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(d => d.City)
            .HasColumnName("city")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(20);

        builder.Property(d => d.Province)
            .HasColumnName("province")
            .HasMaxLength(100);

        builder.Property(d => d.CountryCode)
            .HasColumnName("country_code")
            .HasColumnType("CHAR(2)")
            .IsRequired();

        builder.Property(d => d.UnLocode)
            .HasColumnName("un_locode")
            .HasMaxLength(10);

        builder.Property(d => d.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(d => d.AutoCreated)
            .HasColumnName("auto_created")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        // Relazioni
        builder.HasOne(d => d.Customer)
            .WithMany(c => c.Destinations)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.TransportOperations)
            .WithOne(t => t.Destination)
            .HasForeignKey(t => t.DestinationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
