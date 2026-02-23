using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità AuditLog.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(a => a.EntityType);

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.HasIndex(a => a.EntityId);

        builder.Property(a => a.ActionType)
            .HasColumnName("action_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.PerformedByUserId)
            .HasColumnName("performed_by_user_id")
            .HasColumnType("CHAR(36)");

        builder.HasIndex(a => a.PerformedByUserId);

        builder.Property(a => a.PerformedBySourceId)
            .HasColumnName("performed_by_source_id")
            .HasColumnType("CHAR(36)");

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.OldValueJson)
            .HasColumnName("old_value_json")
            .HasColumnType("JSON");

        builder.Property(a => a.NewValueJson)
            .HasColumnName("new_value_json")
            .HasColumnType("JSON");

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45); // IPv6 length

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.HasIndex(a => a.CreatedAt);

        // Relazioni
        builder.HasOne(a => a.PerformedByUser)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indice composito per query comuni
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.CreatedAt });
    }
}
