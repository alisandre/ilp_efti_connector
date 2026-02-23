using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

public class TransportConsigneeConfiguration : IEntityTypeConfiguration<TransportConsignee>
{
    public void Configure(EntityTypeBuilder<TransportConsignee> builder)
    {
        builder.ToTable("transport_consignees");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id").HasColumnType("CHAR(36)");

        builder.Property(c => c.TransportOperationId)
            .HasColumnName("transport_operation_id").HasColumnType("CHAR(36)").IsRequired();

        builder.HasIndex(c => c.TransportOperationId).IsUnique();

        builder.Property(c => c.Name)
            .HasColumnName("name").HasMaxLength(300).IsRequired();

        builder.Property(c => c.PlayerType)
            .HasColumnName("player_type").HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.OwnsOne(c => c.PostalAddress, addr =>
        {
            addr.Property(a => a.StreetName).HasColumnName("street_name").HasMaxLength(300);
            addr.Property(a => a.PostCode).HasColumnName("post_code").HasMaxLength(20);
            addr.Property(a => a.CityName).HasColumnName("city_name").HasMaxLength(200).IsRequired();
            addr.Property(a => a.CountryCode).HasColumnName("country_code").HasColumnType("CHAR(2)").IsRequired();
            addr.Property(a => a.CountryName).HasColumnName("country_name").HasMaxLength(100);
        });

        builder.Property(c => c.TaxRegistration)
            .HasColumnName("tax_registration").HasMaxLength(100);

        builder.Property(c => c.EoriCode)
            .HasColumnName("eori_code").HasMaxLength(20);

        builder.HasOne(c => c.TransportOperation)
            .WithOne(t => t.Consignee)
            .HasForeignKey<TransportConsignee>(c => c.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
