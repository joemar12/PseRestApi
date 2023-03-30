using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Infrastructure.Persistence.Configurations;

public class HistoricalTradingDataEntityConfiguration : IEntityTypeConfiguration<HistoricalTradingData>
{
    public void Configure(EntityTypeBuilder<HistoricalTradingData> builder)
    {
        builder
            .ToTable("HistoricalTradingData");

        builder
            .HasKey(x => x.Id);

        builder
            .HasOne(x => x.SecurityInfo)
            .WithMany(x => x.HistoricalTradingData)
            .HasForeignKey(x => x.SecurityId);

        builder
            .HasIndex(x => x.Symbol);
    }
}
