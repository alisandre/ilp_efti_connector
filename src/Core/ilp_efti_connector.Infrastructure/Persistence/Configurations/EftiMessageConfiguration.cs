using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità EftiMessage.
/// </summary>
public class EftiMessageConfiguration : IEntityTypeConfiguration<EftiMessage>
{
    public void Configure(EntityTypeBuilder<EftiMessage> builder)
    {
        builder.ToTable("efti_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(m => m.SourceId)
            .HasColumnName("source_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(m => m.TransportOperationId)
            .HasColumnName("transport_operation_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(m => m.CorrelationId)
            .HasColumnName("correlation_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.HasIndex(m => m.CorrelationId);

        builder.Property(m => m.GatewayProvider)
            .HasColumnName("gateway_provider")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.Direction)
            .HasColumnName("direction")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.DatasetType)
            .HasColumnName("dataset_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(m => m.Status);

        builder.Property(m => m.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("JSON")
            .IsRequired();

        builder.Property(m => m.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.HasIndex(m => m.ExternalId);

        builder.Property(m => m.ExternalUuid)
            .HasColumnName("external_uuid")
            .HasMaxLength(100);

        builder.Property(m => m.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue((short)0)
            .IsRequired();

        builder.Property(m => m.NextRetryAt)
            .HasColumnName("next_retry_at")
            .HasColumnType("DATETIME");

        builder.HasIndex(m => m.NextRetryAt);

        builder.Property(m => m.SentAt)
            .HasColumnName("sent_at")
            .HasColumnType("DATETIME");

        builder.Property(m => m.AcknowledgedAt)
            .HasColumnName("acknowledged_at")
            .HasColumnType("DATETIME");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.HasIndex(m => m.CreatedAt);

        // Relazioni
        builder.HasOne(m => m.Source)
            .WithMany(s => s.EftiMessages)
            .HasForeignKey(m => m.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.TransportOperation)
            .WithMany(t => t.EftiMessages)
            .HasForeignKey(m => m.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
