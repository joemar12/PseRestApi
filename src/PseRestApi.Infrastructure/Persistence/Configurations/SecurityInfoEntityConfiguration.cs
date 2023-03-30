using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PseRestApi.Domain.Entities;

namespace PseRestApi.Infrastructure.Persistence.Configurations;
public class SecurityInfoEntityConfiguration : IEntityTypeConfiguration<SecurityInfo>
{
    public void Configure(EntityTypeBuilder<SecurityInfo> builder)
    {
        builder
            .ToTable("SecurityInfo");

        builder
            .HasKey(x => x.SecurityId);

        builder.Property(x => x.CompanyName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SecurityName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SecurityStatus)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.Symbol)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(x => x.Symbol)
            .IsUnique();
    }
}
