using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LocalLink.Domain.Entities;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class SystemInfoConfiguration : IEntityTypeConfiguration<SystemInfo>
{
    public void Configure(EntityTypeBuilder<SystemInfo> builder)
    {
        builder.ToTable("SystemInfos");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(s => s.Key)
            .IsUnique();
    }
}
