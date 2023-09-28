using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Infrastructure.Persistence.Configurations;

public class SyncBatchDataEntityConfiguration : IEntityTypeConfiguration<SyncBatchData>
{
    public void Configure(EntityTypeBuilder<SyncBatchData> builder)
    {
        builder.ToTable("SyncBatchData");

        builder.HasKey(x => x.Id);
    }
}
