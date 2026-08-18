using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired(false);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(a => a.EntityType)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(a => a.EntityId)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(64)
            .IsUnicode(false);

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.CreatedAtUtc);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        builder.HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
