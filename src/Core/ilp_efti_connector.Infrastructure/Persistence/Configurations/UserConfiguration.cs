using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità User.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.KeycloakId)
            .HasColumnName("keycloak_id")
            .HasMaxLength(100);

        builder.HasIndex(u => u.KeycloakId);

        builder.Property(u => u.RolesJson)
            .HasColumnName("roles_json")
            .HasColumnType("JSON");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("DATETIME");

        // Relazioni
        builder.HasMany(u => u.CreatedTransportOperations)
            .WithOne(t => t.CreatedByUser)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.UpdatedTransportOperations)
            .WithOne(t => t.UpdatedByUser)
            .HasForeignKey(t => t.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.AuditLogs)
            .WithOne(a => a.PerformedByUser)
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
