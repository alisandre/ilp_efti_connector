using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità TransportOperation.
/// </summary>
public class TransportOperationConfiguration : IEntityTypeConfiguration<TransportOperation>
{
    public void Configure(EntityTypeBuilder<TransportOperation> builder)
    {
        builder.ToTable("transport_operations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(t => t.SourceId)
            .HasColumnName("source_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(t => t.CustomerId)
            .HasColumnName("customer_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(t => t.DestinationId)
            .HasColumnName("destination_id")
            .HasColumnType("CHAR(36)");

        builder.Property(t => t.OperationCode)
            .HasColumnName("operation_code")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.OperationCode);

        builder.Property(t => t.DatasetType)
            .HasColumnName("dataset_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Hashcode)
            .HasColumnName("hashcode")
            .HasMaxLength(64);

        builder.Property(t => t.HashcodeAlgorithm)
            .HasColumnName("hashcode_algorithm")
            .HasMaxLength(20);

        builder.Property(t => t.RawPayloadJson)
            .HasColumnName("raw_payload_json")
            .HasColumnType("JSON");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.Property(t => t.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasColumnType("CHAR(36)");

        builder.Property(t => t.UpdatedByUserId)
            .HasColumnName("updated_by_user_id")
            .HasColumnType("CHAR(36)");

        // Relazioni
        builder.HasOne(t => t.Source)
            .WithMany(s => s.TransportOperations)
            .HasForeignKey(t => t.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Customer)
            .WithMany(c => c.TransportOperations)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Destination)
            .WithMany(d => d.TransportOperations)
            .HasForeignKey(t => t.DestinationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTransportOperations)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.UpdatedByUser)
            .WithMany(u => u.UpdatedTransportOperations)
            .HasForeignKey(t => t.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.EftiMessages)
            .WithOne(m => m.TransportOperation)
            .HasForeignKey(m => m.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relazioni verso le tabelle figlio strutturate (sostituiscono le colonne JSON)
        builder.HasOne(t => t.Consignee)
            .WithOne(c => c.TransportOperation)
            .HasForeignKey<TransportConsignee>(c => c.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Detail)
            .WithOne(d => d.TransportOperation)
            .HasForeignKey<TransportDetail>(d => d.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ConsignmentItem)
            .WithOne(i => i.TransportOperation)
            .HasForeignKey<TransportConsignmentItem>(i => i.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Carriers)
            .WithOne(c => c.TransportOperation)
            .HasForeignKey(c => c.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
