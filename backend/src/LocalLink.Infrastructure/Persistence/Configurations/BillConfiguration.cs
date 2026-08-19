using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("Bills", t =>
        {
            t.HasCheckConstraint("CK_Bills_Amount_Positive", "[Amount] > 0");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CustomerId)
            .IsRequired();

        builder.Property(b => b.ProviderName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.BillType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(b => b.BillNumber)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(b => b.BillNumber)
            .IsUnique();

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.DueDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.Property(b => b.UpdatedAtUtc)
            .IsRequired(false);

        builder.HasIndex(b => new { b.CustomerId, b.Status });
        builder.HasIndex(b => b.DueDate);

        builder.HasOne(b => b.Customer)
            .WithMany(c => c.Bills)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
