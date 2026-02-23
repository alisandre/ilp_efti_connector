using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Infrastructure.Persistence.Configurations;

public class TransportDetailConfiguration : IEntityTypeConfiguration<TransportDetail>
{
    public void Configure(EntityTypeBuilder<TransportDetail> builder)
    {
        builder.ToTable("transport_details");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id").HasColumnType("CHAR(36)");

        builder.Property(d => d.TransportOperationId)
            .HasColumnName("transport_operation_id").HasColumnType("CHAR(36)").IsRequired();

        builder.HasIndex(d => d.TransportOperationId).IsUnique();

        builder.Property(d => d.CargoType)
            .HasColumnName("cargo_type").HasConversion<string>().HasMaxLength(20);

        builder.Property(d => d.Incoterms)
            .HasColumnName("incoterms").HasConversion<string>().HasMaxLength(5);

        // Luogo di presa in carico contrattuale (contractualCarrierAcceptanceLocation)
        builder.OwnsOne(d => d.AcceptanceAddress, addr =>
        {
            addr.Property(a => a.StreetName).HasColumnName("acceptance_street_name").HasMaxLength(300);
            addr.Property(a => a.PostCode).HasColumnName("acceptance_post_code").HasMaxLength(20);
            addr.Property(a => a.CityName).HasColumnName("acceptance_city_name").HasMaxLength(200);
            addr.Property(a => a.CountryCode).HasColumnName("acceptance_country_code").HasColumnType("CHAR(2)");
            addr.Property(a => a.CountryName).HasColumnName("acceptance_country_name").HasMaxLength(100);
        });

        builder.Property(d => d.AcceptanceDate)
            .HasColumnName("acceptance_date").HasColumnType("DATETIME");

        // Luogo di consegna contrattuale (contractualConsigneeReceiptLocation)
        builder.OwnsOne(d => d.ReceiptAddress, addr =>
        {
            addr.Property(a => a.StreetName).HasColumnName("receipt_street_name").HasMaxLength(300);
            addr.Property(a => a.PostCode).HasColumnName("receipt_post_code").HasMaxLength(20);
            addr.Property(a => a.CityName).HasColumnName("receipt_city_name").HasMaxLength(200);
            addr.Property(a => a.CountryCode).HasColumnName("receipt_country_code").HasColumnType("CHAR(2)");
            addr.Property(a => a.CountryName).HasColumnName("receipt_country_name").HasMaxLength(100);
        });

        builder.HasOne(d => d.TransportOperation)
            .WithOne(t => t.Detail)
            .HasForeignKey<TransportDetail>(d => d.TransportOperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
