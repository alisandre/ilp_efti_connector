using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità Customer.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(c => c.CustomerCode)
            .HasColumnName("customer_code")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.CustomerCode)
            .IsUnique();

        builder.Property(c => c.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(c => c.VatNumber)
            .HasColumnName("vat_number")
            .HasMaxLength(50);

        builder.Property(c => c.EoriCode)
            .HasColumnName("eori_code")
            .HasMaxLength(20);

        builder.Property(c => c.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(255);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(c => c.AutoCreated)
            .HasColumnName("auto_created")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.SourceId)
            .HasColumnName("source_id")
            .HasColumnType("CHAR(36)");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        // Relazioni
        builder.HasOne(c => c.Source)
            .WithMany(s => s.Customers)
            .HasForeignKey(c => c.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Destinations)
            .WithOne(d => d.Customer)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.TransportOperations)
            .WithOne(t => t.Customer)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
