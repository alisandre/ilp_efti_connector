using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

public class TransportPackageConfiguration : IEntityTypeConfiguration<TransportPackage>
{
    public void Configure(EntityTypeBuilder<TransportPackage> builder)
    {
        builder.ToTable("transport_packages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id").HasColumnType("CHAR(36)");

        builder.Property(p => p.ConsignmentItemId)
            .HasColumnName("consignment_item_id").HasColumnType("CHAR(36)").IsRequired();

        builder.Property(p => p.SortOrder)
            .HasColumnName("sort_order").IsRequired();

        builder.Property(p => p.ShippingMarks)
            .HasColumnName("shipping_marks").HasMaxLength(100);

        builder.Property(p => p.ItemQuantity)
            .HasColumnName("item_quantity").IsRequired();

        builder.Property(p => p.TypeCode)
            .HasColumnName("type_code").HasMaxLength(50);

        builder.Property(p => p.GrossWeight)
            .HasColumnName("gross_weight").HasColumnType("DECIMAL(10,3)").IsRequired();

        builder.Property(p => p.GrossVolume)
            .HasColumnName("gross_volume").HasColumnType("DECIMAL(10,3)");

        builder.HasOne(p => p.ConsignmentItem)
            .WithMany(i => i.Packages)
            .HasForeignKey(p => p.ConsignmentItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
