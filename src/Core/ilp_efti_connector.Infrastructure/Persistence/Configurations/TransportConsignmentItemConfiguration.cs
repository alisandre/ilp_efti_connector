using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

public class TransportConsignmentItemConfiguration : IEntityTypeConfiguration<TransportConsignmentItem>
{
    public void Configure(EntityTypeBuilder<TransportConsignmentItem> builder)
    {
        builder.ToTable("transport_consignment_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id").HasColumnType("CHAR(36)");

        builder.Property(i => i.TransportOperationId)
            .HasColumnName("transport_operation_id").HasColumnType("CHAR(36)").IsRequired();

        builder.HasIndex(i => i.TransportOperationId).IsUnique();

        builder.Property(i => i.TotalItemQuantity)
            .HasColumnName("total_item_quantity").IsRequired();

        builder.Property(i => i.TotalWeight)
            .HasColumnName("total_weight").HasColumnType("DECIMAL(10,3)").IsRequired();

        builder.Property(i => i.TotalVolume)
            .HasColumnName("total_volume").HasColumnType("DECIMAL(10,3)");

        builder.HasOne(i => i.TransportOperation)
            .WithOne(t => t.ConsignmentItem)
            .HasForeignKey<TransportConsignmentItem>(i => i.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Packages)
            .WithOne(p => p.ConsignmentItem)
            .HasForeignKey(p => p.ConsignmentItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
