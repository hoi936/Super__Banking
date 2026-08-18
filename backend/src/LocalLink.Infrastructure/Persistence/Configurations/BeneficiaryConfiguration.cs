using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("Beneficiaries");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CustomerId)
            .IsRequired();

        builder.Property(b => b.BeneficiaryAccountId)
            .IsRequired();

        builder.Property(b => b.Nickname)
            .HasMaxLength(100);

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(b => new { b.CustomerId, b.BeneficiaryAccountId })
            .IsUnique();

        builder.HasOne(b => b.Customer)
            .WithMany(c => c.Beneficiaries)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BeneficiaryAccount)
            .WithMany(ba => ba.BeneficiaryRecords)
            .HasForeignKey(b => b.BeneficiaryAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
