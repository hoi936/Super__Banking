using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class GatewayTransactionConfiguration : IEntityTypeConfiguration<GatewayTransaction>
{
    public void Configure(EntityTypeBuilder<GatewayTransaction> builder)
    {
        builder.ToTable("GatewayTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(t => t.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(t => t.GatewayTransactionId)
            .HasMaxLength(100);

        builder.HasIndex(t => t.ReferenceNumber).IsUnique();
        
        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
