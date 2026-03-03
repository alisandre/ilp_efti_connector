using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configurazione Entity Framework per l'entità Source.
/// </summary>
public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => s.Code)
            .IsUnique();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.ApiKeyHash)
            .HasColumnName("api_key_hash")
            .HasMaxLength(64);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(s => s.ConfigJson)
            .HasColumnName("config_json")
            .HasColumnType("JSON");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("DATETIME")
            .IsRequired();

        // Relazioni
        builder.HasMany(s => s.Customers)
            .WithOne(c => c.Source)
            .HasForeignKey(c => c.SourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.TransportOperations)
            .WithOne(t => t.Source)
            .HasForeignKey(t => t.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.EftiMessages)
            .WithOne(m => m.Source)
            .HasForeignKey(m => m.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed — sorgente di test per l'ambiente di sviluppo
        builder.HasData(new Source
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Code = "TMS_TEST",
            Name = "Sorgente di Test",
            Type = ilp_efti_connector.Domain.Enums.SourceType.TMS,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed — sorgente di test per l'ambiente di sviluppo
        builder.HasData(new Source
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Code = "TEST_FRONTEND",
            Name = "Form di test per inserimento",
            Type = ilp_efti_connector.Domain.Enums.SourceType.MANUAL,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
